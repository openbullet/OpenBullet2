using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections;
using Renci.SshNet.Messages;
using System;

namespace RuriLib.Models.Hits.HitOutputs
{
    public class MongoDBHitOutput : IHitOutput
    {
        public string ClusterURL { get; set; }
        public string CollectionName { get; set; }
        public string DatabaseName { get; set; }
        public bool OnlyHits { get; set; }

        public MongoDBHitOutput(string clusterurl = "", string collectionname = "",string databasename = "",bool onlyHits = true)
        {
            ClusterURL = clusterurl;
            DatabaseName = databasename;
            CollectionName = collectionname;
            OnlyHits = onlyHits;
        }

        public async Task Store(Hit hit)
        {
            if (OnlyHits && hit.Type != "SUCCESS")
            {
                return;
            }
            try
            {
                var settings = MongoClientSettings.FromConnectionString(ClusterURL);
                settings.ServerApi = new ServerApi(ServerApiVersion.V1);
                var client = new MongoClient(settings);
                var database = client.GetDatabase(DatabaseName);
                var collection = database.GetCollection<BsonDocument>(CollectionName);
                var hitData = new BsonDocument { { "Hit", hit.ToString() } };
                await collection.InsertOneAsync(hitData);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Ayo I fucked up here" + ex.Message);
            }
        }
    }
}
