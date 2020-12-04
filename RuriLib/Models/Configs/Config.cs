using RuriLib.Helpers.Transpilers;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
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

        // Checks if the config has only blocks or also additional C# code
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
    }
}
