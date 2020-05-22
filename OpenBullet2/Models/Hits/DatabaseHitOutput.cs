using Microsoft.AspNetCore.Components;
using OpenBullet2.Entities;
using OpenBullet2.Repositories;
using RuriLib.Models.Data.DataPools;
using RuriLib.Models.Hits;
using RuriLib.Models.Variables;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Models.Hits
{
    public class DatabaseHitOutput : IHitOutput
    {
        [Inject] IHitRepository HitRepo { get; set; }

        public async Task Store(Hit hit)
        {
            var entity = new HitEntity
            {
                CapturedData = ConvertCapturedData(hit.CapturedData),
                Data = hit.Data.Data,
                Date = hit.Date,
                Proxy = hit.Proxy.ToString(),
                Type = hit.Type,
                ConfigId = hit.Config.Id,
                ConfigName = hit.Config.Metadata.Name,
                ConfigCategory = hit.Config.Metadata.Category
            };

            if (hit.DataPool is WordlistDataPool)
            {
                var wordlist = (hit.DataPool as WordlistDataPool).Wordlist;
                entity.WordlistId = wordlist.Id;
                entity.WordlistName = wordlist.Name;
            }

            await HitRepo.Add(entity);
        }

        private string ConvertCapturedData(Dictionary<string, object> capturedData)
        {
            List<Variable> variables = new List<Variable>();
            VariableFactory factory = new VariableFactory();

            foreach (var data in capturedData)
            {
                var variable = factory.FromObject(data.Value);
                variable.Name = data.Key;
                variables.Add(variable);
            }

            return string.Join(" | ", variables.Select(v => v.AsString()));
        }
    }
}
