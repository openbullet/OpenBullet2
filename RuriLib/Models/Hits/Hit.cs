using RuriLib.Helpers.Blocks;
using RuriLib.Logging;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Proxies;
using RuriLib.Models.Variables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Models.Hits
{
    public class Hit
    {
        public DataLine Data { get; set; }
        public string DataString => Data.Data;
        public Dictionary<string, object> CapturedData { get; set; }
        public string CapturedDataString => ConvertCapturedData();
        public Proxy Proxy { get; set; }
        public string ProxyString => Proxy == null ? "" : Proxy.ToString();
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public Config Config { get; set; }
        public DataPool DataPool { get; set; }
        public IBotLogger BotLogger { get; set; }
        public int OwnerId { get; set; } = -1;

        public override string ToString() => $"{DataString} | {CapturedDataString}";

        private string ConvertCapturedData()
        {
            List<Variable> variables = new List<Variable>();
            VariableFactory factory = new VariableFactory();

            foreach (var data in CapturedData)
            {
                var variable = factory.FromObject(data.Value);
                variable.Name = data.Key;
                variables.Add(variable);
            }

            return string.Join(" | ", variables.Select(v => $"{v.Name} = {v.AsString()}"));
        }
    }
}
