using RuriLib.Functions.Conversion;
using RuriLib.Functions.Crypto;
using RuriLib.Helpers.Transpilers;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace RuriLib.Models.Configs
{
    public class Config
    {
        public string Id { get; set; }
        public ConfigMode Mode { get; set; } = ConfigMode.Stack;
        public ConfigMetadata Metadata { get; set; } = new ConfigMetadata();
        public ConfigSettings Settings { get; set; } = new ConfigSettings();
        public string Readme { get; set; } = "Type some **markdown** here";
        
        public List<BlockInstance> Stack { get; set; } = new List<BlockInstance>();
        public string LoliCodeScript { get; set; } = "";
        public string CSharpScript { get; set; } = "";

        // Hashes used to check if the config was saved
        private string loliCodeHash;
        private string cSharpHash;

        [JsonIgnore]
        public List<(BlockInstance, int)> DeletedBlocksHistory { get; set; } = new List<(BlockInstance, int)>();

        public Config()
        {
            Id = Guid.NewGuid().ToString();
        }

        public void ChangeMode(ConfigMode newMode)
        {
            if (newMode == Mode)
                return;

            var mappings = new Dictionary<(ConfigMode, ConfigMode), Action>
            {
                { (ConfigMode.Stack, ConfigMode.LoliCode), () => LoliCodeScript = new Stack2LoliTranspiler().Transpile(Stack) },
                { (ConfigMode.Stack, ConfigMode.CSharp), () => CSharpScript = new Stack2CSharpTranspiler().Transpile(Stack, Settings) },
                { (ConfigMode.LoliCode, ConfigMode.Stack), () => Stack = new Loli2StackTranspiler().Transpile(LoliCodeScript) },
                { (ConfigMode.LoliCode, ConfigMode.CSharp), () => CSharpScript = new Stack2CSharpTranspiler()
                    .Transpile(new Loli2StackTranspiler().Transpile(LoliCodeScript), Settings) }
            };

            if (mappings.ContainsKey((Mode, newMode)))
            {
                mappings[(Mode, newMode)].Invoke();
                Mode = newMode;
            }
            else
            {
                throw new Exception($"Cannot convert mode from {Mode} to {newMode}");
            }
        }

        /// <summary>
        /// Checks if the config has only blocks or also additional C# code
        /// </summary>
        public bool HasCSharpCode()
        {
            try
            {
                return Mode switch
                {
                    ConfigMode.CSharp => true,
                    ConfigMode.Stack => Stack.Any(b => (b is LoliCodeBlockInstance || b is ScriptBlockInstance)),
                    ConfigMode.LoliCode => new Loli2StackTranspiler().Transpile(LoliCodeScript)
                        .Any(b => (b is LoliCodeBlockInstance || b is ScriptBlockInstance)),
                    _ => throw new NotImplementedException(),
                };
            }
            catch
            {
                // We don't know, return false just to be safe
                return false;
            }
        }

        /// <summary>
        /// Update the hashes of the current state of LoliCode and C# scripts
        /// (call this this when you first load the config or when you save changes to the repository).
        /// </summary>
        public void UpdateHashes()
        {
            loliCodeHash = HexConverter.ToHexString(Crypto.SHA1(Encoding.UTF8.GetBytes(LoliCodeScript)));
            cSharpHash = HexConverter.ToHexString(Crypto.SHA1(Encoding.UTF8.GetBytes(CSharpScript)));
        }

        /// <summary>
        /// Checks if the config's LoliCode or C# code have been edited since
        /// the last call of <see cref="UpdateHashes"/>.
        /// </summary>
        public bool HasUnsavedChanges()
            => Mode == ConfigMode.CSharp
                ? HexConverter.ToHexString(Crypto.SHA1(Encoding.UTF8.GetBytes(CSharpScript))) != cSharpHash
                : HexConverter.ToHexString(Crypto.SHA1(Encoding.UTF8.GetBytes(LoliCodeScript))) != loliCodeHash;
    }
}
