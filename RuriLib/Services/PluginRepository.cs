using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RuriLib.Helpers.Blocks;

namespace RuriLib.Services;

/// <summary>
/// Manages plugin discovery, installation, and deletion.
/// </summary>
public class PluginRepository
{
    // AppDomains other than AppDomain.CurrentDomain aren't supported in .NET core
    private readonly AppDomain _domain = AppDomain.CurrentDomain;
    private readonly List<string> _toDelete = [];
    private readonly ILogger<PluginRepository> logger;
    private string BaseFolder { get; init; }
    private string ToDeleteFile => Path.Combine(BaseFolder, ".toDelete");

    /// <summary>
    /// Creates a repository rooted at the given base folder.
    /// </summary>
    /// <param name="baseFolder">The folder that contains plugin assemblies and dependencies.</param>
    /// <param name="logger">The optional logger.</param>
    public PluginRepository(string baseFolder, ILogger<PluginRepository>? logger = null)
    {
        this.logger = logger ?? NullLogger<PluginRepository>.Instance;
        BaseFolder = baseFolder;
        Directory.CreateDirectory(baseFolder);

        // Hook the EventHandler for assembly resolution
        _domain.AssemblyResolve += ResolveHandler;

        // Delete plugins that were marked for deletion
        if (File.Exists(ToDeleteFile))
        {
            var toDelete = File.ReadAllLines(ToDeleteFile).Where(p => !string.IsNullOrWhiteSpace(p));

            foreach (var pluginName in toDelete)
            {
                var path = Path.Combine(baseFolder, pluginName);
                var filePath = $"{path}.dll";

                // Delete the dll file if it exists
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Delete the directory if it exists
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }

            File.Delete(ToDeleteFile);
        }

        // Load all existing plugins and their dependencies in the AppDomain
        LoadAssemblies(GetPlugins());
        ReloadBlockDescriptors();
    }

    /// <summary>
    /// Gets assemblies from .dll files in the base folder.
    /// </summary>
    /// <returns>The loaded plugin assemblies.</returns>
    public IEnumerable<Assembly> GetPlugins()
        => Directory.GetFiles(BaseFolder, "*.dll")
            .Where(p => !_toDelete.Contains(Path.GetFileNameWithoutExtension(p)))
            .Select(Assembly.LoadFrom);

    /// <summary>
    /// Retrieves the names of .dll files in the base folder (without extension).
    /// </summary>
    /// <returns>The plugin names.</returns>
    public IEnumerable<string> GetPluginNames()
        => Directory.GetFiles(BaseFolder, "*.dll")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !string.IsNullOrWhiteSpace(name) && !_toDelete.Contains(name))!;

    /// <summary>
    /// Retrieves the assemblies of all plugins and their references.
    /// </summary>
    /// <returns>The plugin assemblies and their referenced assemblies.</returns>
    public IEnumerable<Assembly> GetPluginsAndReferences()
        => GetReferences(GetPlugins());

    /// <summary>
    /// Adds a plugin from a .zip file.
    /// </summary>
    /// <param name="stream">The ZIP archive stream containing the plugin payload.</param>
    public void AddPlugin(Stream stream)
    {
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, false);

        // Make sure there's at least one .dll in the root of the archive
        var dlls = archive.Entries.Where(e => !e.FullName.Contains('/') && e.FullName.EndsWith(".dll"));

        if (!dlls.Any())
        {
            throw new FileNotFoundException("No dll file found in the root of the provided archive!");
        }

        // If we're trying to import a plugin we previously deleted, warn the user to restart first
        if (dlls.Any(e => _toDelete.Contains(Path.GetFileNameWithoutExtension(e.Name))))
        {
            throw new Exception("Please restart the application and try again");
        }

        foreach (var entry in archive.Entries)
        {
            try
            {
                var folder = Path.Combine(BaseFolder, Path.GetDirectoryName(entry.FullName) ?? string.Empty);
                Directory.CreateDirectory(folder);
                entry.ExtractToFile(Path.Combine(BaseFolder, entry.FullName), true);
            }
            catch
            {
                // ignored
            }
        }

        // Load new assemblies into the domain (the ones that are already loaded will be skipped)
        LoadAssemblies(GetPlugins());
        ReloadBlockDescriptors();
    }

    // Delete a plugin (unload, recreate Descriptors) (also unload all deps from appdomain?)
    /// <summary>
    /// Marks a plugin for deletion and refreshes the descriptors repository.
    /// </summary>
    /// <param name="name">The plugin name.</param>
    public void DeletePlugin(string name)
    {
        // TODO: Loading and unloading through AssemblyLoadContext
        // https://github.com/dotnet/samples/blob/master/core/tutorials/Unloading/Host/Program.cs

        // If already marked for deletion, skip
        if (_toDelete.Contains(name))
        {
            return;
        }

        // Append the plugin's name to the deletion list
        File.AppendAllText(ToDeleteFile, name + Environment.NewLine);
        _toDelete.Add(name);

        ReloadBlockDescriptors();
    }

    // Retrieves the path of folders that contain the dependencies of existing plugins.
    private IEnumerable<string> GetDependencyFolders()
        => GetPluginNames().Select(p => Path.Combine(BaseFolder, p));

    // Builds a list of assemblies and their references recursively
    private IEnumerable<Assembly> GetReferences(IEnumerable<Assembly> assemblies)
        => assemblies.Concat(GetReferences(assemblies.SelectMany(a => a.GetReferencedAssemblies()).Select(n => Assembly.Load(n))));

    // Recreates the descriptors repository and loads the plugins
    private void ReloadBlockDescriptors()
    {
        var repository = new DescriptorsRepository();
        GetPlugins().ToList().ForEach(p => repository.AddFromExposedMethods(p));
        Globals.DescriptorsRepository = repository;
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
                    _domain.Load(asm);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Couldn't load required dependency {AssemblyFullName}", asm.FullName);
                }

                // Load its dependencies recursively
                LoadAssemblies(Assembly.Load(asm).GetReferencedAssemblies());
            }
        }
    }

    // Checks if an assembly is already loaded in the domain
    private bool IsAlreadyLoaded(AssemblyName assembly)
        => _domain.GetAssemblies().Any(a => a.FullName == assembly.FullName);

    // Handler that resolves assemblies from the Plugins folder and its subdirectories
    private Assembly? ResolveHandler(object? sender, ResolveEventArgs args)
    {
        // Check if the requested assembly is part of the loaded assemblies
        var loadedAssembly = _domain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);

        if (loadedAssembly != null)
        {
            return loadedAssembly;
        }

        // This resolver is called when an loaded control tries to load a generated XmlSerializer - We need to discard it.
        // http://connect.microsoft.com/VisualStudio/feedback/details/88566/bindingfailure-an-assembly-failed-to-load-while-using-xmlserialization

        var n = new AssemblyName(args.Name);

        if (n.Name?.EndsWith(".xmlserializers", StringComparison.OrdinalIgnoreCase) == true)
        {
            return null;
        }

        // http://stackoverflow.com/questions/4368201/appdomain-currentdomain-assemblyresolve-asking-for-a-appname-resources-assembl

        if (n.Name?.EndsWith(".resources", StringComparison.OrdinalIgnoreCase) == true)
        {
            return null;
        }

        // Get the folders where assemblies of plugins can be found
        var folders = GetDependencyFolders().ToList();
        folders.Add(BaseFolder);

        string? assy = null;

        // Find the corresponding assembly file
        foreach (var dir in folders)
        {
            assy = new[] { "*.dll", "*.exe" }.SelectMany(g => Directory.EnumerateFiles(dir, g)).FirstOrDefault(f =>
            {
                try { return string.Equals(n.Name, AssemblyName.GetAssemblyName(f).Name, StringComparison.OrdinalIgnoreCase); }
                catch (BadImageFormatException) { return false; /* Bypass assembly is not a .net exe */ }
                catch (Exception ex) { throw new ApplicationException($"Error loading assembly {f}", ex); }
            });

            if (assy != null)
            {
                return Assembly.LoadFrom(assy);
            }
        }

        throw new ApplicationException($"Assembly {args.Name} not found");
    }
}
