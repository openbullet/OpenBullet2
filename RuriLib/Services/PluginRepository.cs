using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace RuriLib.Services
{
    public class PluginRepository
    {
        private readonly AppDomain domain = AppDomain.CurrentDomain;
        private readonly string baseFolder = "Plugins";

        public PluginRepository()
        {
            // Create the base folder if it doesn't exist
            Directory.CreateDirectory(baseFolder);

            // Hook the EventHandler for assembly resolution
            domain.AssemblyResolve += ResolveHandler;

            // Load all existing plugins and their dependencies in the AppDomain
            LoadAssemblies(GetPlugins());
            ReloadBlockDescriptors();
        }

        /// <summary>
        /// Gets assemblies from .dll files in the base folder.
        /// </summary>
        public IEnumerable<Assembly> GetPlugins()
            => Directory.GetFiles(baseFolder, "*.dll").Select(p => Assembly.LoadFrom(p));

        /// <summary>
        /// Retrieves the names of .dll files in the base folder (without extension).
        /// </summary>
        public IEnumerable<string> GetPluginNames()
            => Directory.GetFiles(baseFolder, "*.dll").Select(p => Path.GetFileNameWithoutExtension(p));

        /// <summary>
        /// Retrieves the path of folders that contain the dependencies of existing plugins.
        /// </summary>
        private IEnumerable<string> GetDependencyFolders()
            => GetPluginNames().Select(p => Path.Combine(baseFolder, p));

        /// <summary>
        /// Retrieves the assemblies of all plugins and their references.
        /// </summary>
        public IEnumerable<Assembly> GetPluginsAndReferences()
            => GetReferences(GetPlugins());

        // Builds a list of assemblies and their references recursively
        private IEnumerable<Assembly> GetReferences(IEnumerable<Assembly> assemblies)
            => assemblies.Concat(GetReferences(assemblies.SelectMany(a => a.GetReferencedAssemblies()).Select(n => Assembly.Load(n))));

        /// <summary>
        /// Adds a plugin from a .zip file.
        /// </summary>
        public void AddPlugin(Stream stream)
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, false);

           // Make sure there's at least one .dll in the root of the archive
            if (!archive.Entries.Any(e => !e.FullName.Contains('/') && e.FullName.EndsWith(".dll")))
                throw new FileNotFoundException("No dll file found in the root of the provided archive!");

            archive.ExtractToDirectory(baseFolder);

            // Load new assemblies into the domain (the ones that are already loaded will be skipped)
            LoadAssemblies(GetPlugins());
            ReloadBlockDescriptors();
        }

        // Delete a plugin (unload, recreate Descriptors) (also unload all deps from appdomain?)
        public void DeletePlugin(string name)
        {
            // TODO: Unload the assembly from the AppDomain (leave its dependencies loaded
            // because maybe they are the same that are being used by RuriLib).

            var folder = Path.Combine(baseFolder, name);

            // Delete the dll file
            File.Delete($"{folder}.dll");

            // If it exists, delete the directory as well
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);

            ReloadBlockDescriptors();
        }

        private void ReloadBlockDescriptors()
        {
            // Recreate the descriptors repository and load the plugins
            Globals.DescriptorsRepository.Recreate();
            GetPlugins().ToList().ForEach(p => Globals.DescriptorsRepository.AddFromExposedMethods(p));
        }

        private void LoadAssemblies(IEnumerable<Assembly> assemblies)
            => LoadAssemblies(assemblies.Select(a => a.GetName()));

        // Loads assemblies and all their dependencies
        private void LoadAssemblies(IEnumerable<AssemblyName> assemblies)
        {
            foreach (var asm in assemblies)
            {
                if (!IsAlreadyLoaded(asm))
                {
                    try
                    {
                        domain.Load(asm);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ERROR: Couldn't load required dependency {asm.FullName} ({ex.Message})");
                    }
                    
                    // Load its dependencies recursively
                    LoadAssemblies(Assembly.Load(asm).GetReferencedAssemblies());
                }
            }
        }

        // Checks if an assembly is already loaded in the domain
        private bool IsAlreadyLoaded(AssemblyName assembly)
            => AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName == assembly.FullName);

        // Handler that resolves assemblies from the Plugins folder and its subdirectories
        private Assembly ResolveHandler(object sender, ResolveEventArgs args)
        {
            // Check if the requested assembly is part of the loaded assemblies
            var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (loadedAssembly != null)
                return loadedAssembly;

            // This resolver is called when an loaded control tries to load a generated XmlSerializer - We need to discard it.
            // http://connect.microsoft.com/VisualStudio/feedback/details/88566/bindingfailure-an-assembly-failed-to-load-while-using-xmlserialization

            var n = new AssemblyName(args.Name);

            if (n.Name.EndsWith(".xmlserializers", StringComparison.OrdinalIgnoreCase))
                return null;

            // http://stackoverflow.com/questions/4368201/appdomain-currentdomain-assemblyresolve-asking-for-a-appname-resources-assembl

            if (n.Name.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))
                return null;

            // Get the folders where assemblies of plugins can be found
            List<string> folders = GetDependencyFolders().ToList();
            folders.Add(baseFolder);

            string assy = null;

            // Find the corresponding assembly file
            foreach (var dir in folders)
            {
                assy = new[] { "*.dll", "*.exe" }.SelectMany(g => Directory.EnumerateFiles(dir, g)).FirstOrDefault(f =>
                {
                    try { return n.Name.Equals(AssemblyName.GetAssemblyName(f).Name, StringComparison.OrdinalIgnoreCase); }
                    catch (BadImageFormatException) { return false; /* Bypass assembly is not a .net exe */ }
                    catch (Exception ex) { throw new ApplicationException($"Error loading assembly {f}", ex); }
                });

                if (assy != null)
                    return Assembly.LoadFrom(assy);
            }

            throw new ApplicationException($"Assembly {args.Name} not found");
        }
    }
}
