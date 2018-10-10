using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using MongoDB.Bson.IO;

namespace NEL_Scan_API.lib
{
    public class mongoHelper
    {
        public string block_mongodbConnStr_testnet = string.Empty;
        public string block_mongodbDatabase_testnet = string.Empty;
        public string analy_mongodbConnStr_testnet = string.Empty;
        public string analy_mongodbDatabase_testnet = string.Empty;
        public string notify_mongodbConnStr_testnet = string.Empty;
        public string notify_mongodbDatabase_testnet = string.Empty;
        public string nelJsonRPCUrl_testnet = string.Empty;

        public string block_mongodbConnStr_mainnet = string.Empty;
        public string block_mongodbDatabase_mainnet = string.Empty;
        public string analy_mongodbConnStr_mainnet = string.Empty;
        public string analy_mongodbDatabase_mainnet = string.Empty;
        public string notify_mongodbConnStr_mainnet = string.Empty;
        public string notify_mongodbDatabase_mainnet = string.Empty;
        public string nelJsonRPCUrl_mainnet = string.Empty;
        
        public string auctionStateColl_testnet = string.Empty;
        public string auctionStateColl_mainnet = string.Empty;
        public string bonusAddress_testnet = string.Empty;
        public string bonusAddress_mainnet = string.Empty;
        
        public string bonusSgas_mongodbConnStr_testnet { set; get; }
        public string bonusSgas_mongodbConnStr_mainnet { set; get; }
        public string bonusSgas_mongodbDatabase_testnet { set; get; }
        public string bonusSgas_mongodbDatabase_mainnet { set; get; }
        public string bonusSgasCol_testnet { set; get; }
        public string bonusSgasCol_mainnet { set; get; }
        public string id_sgas_testnet { set; get; }
        public string id_sgas_mainnet { set; get; }


        public mongoHelper()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection()    //将配置文件的数据加载到内存中
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())   //指定配置文件所在的目录
                .AddJsonFile("mongodbsettings.json", optional: true, reloadOnChange: true)  //指定加载的配置文件
                .Build();    //编译成对象  
            block_mongodbConnStr_testnet = config["block_mongodbConnStr_testnet"];
            block_mongodbDatabase_testnet = config["block_mongodbDatabase_testnet"];
            analy_mongodbConnStr_testnet = config["analy_mongodbConnStr_testnet"];
            analy_mongodbDatabase_testnet = config["analy_mongodbDatabase_testnet"];
            notify_mongodbConnStr_testnet = config["notify_mongodbConnStr_testnet"];
            notify_mongodbDatabase_testnet = config["notify_mongodbDatabase_testnet"];
            nelJsonRPCUrl_testnet = config["nelJsonRPCUrl_testnet"];

            block_mongodbConnStr_mainnet = config["block_mongodbConnStr_mainnet"];
            block_mongodbDatabase_mainnet = config["block_mongodbDatabase_mainnet"];
            analy_mongodbConnStr_mainnet = config["analy_mongodbConnStr_mainnet"];
            analy_mongodbDatabase_mainnet = config["analy_mongodbDatabase_mainnet"];
            notify_mongodbConnStr_mainnet = config["notify_mongodbConnStr_mainnet"];
            notify_mongodbDatabase_mainnet = config["notify_mongodbDatabase_mainnet"];
            nelJsonRPCUrl_mainnet = config["nelJsonRPCUrl_mainnet"];
            
            auctionStateColl_testnet = config["auctionStateColl_testnet"];
            auctionStateColl_mainnet = config["auctionStateColl_mainnet"];
            bonusAddress_testnet = config["bonusAddress_testnet"];
            bonusAddress_mainnet = config["bonusAddress_mainnet"];

            bonusSgas_mongodbConnStr_testnet = config["bonusSgas_mongodbConnStr_testnet"];
            bonusSgas_mongodbConnStr_mainnet = config["bonusSgas_mongodbConnStr_mainnet"];
            bonusSgas_mongodbDatabase_testnet = config["bonusSgas_mongodbDatabase_testnet"];
            bonusSgas_mongodbDatabase_mainnet = config["bonusSgas_mongodbDatabase_mainnet"];
            bonusSgasCol_testnet = config["bonusSgasCol_testnet"];
            bonusSgasCol_mainnet = config["bonusSgasCol_mainnet"];
            id_sgas_testnet = config["id_sgas_testnet"];
            id_sgas_mainnet = config["id_sgas_mainnet"];
        }

        public long GetDataCount(string mongodbConnStr, string mongodbDatabase, string coll, string findBson = "{}")
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);

            var txCount = collection.Find(BsonDocument.Parse(findBson)).CountDocuments();

            client = null;

            return txCount;
        }

        public JArray GetData(string mongodbConnStr, string mongodbDatabase, string coll, string findFliter)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);

            List<BsonDocument> query = collection.Find(BsonDocument.Parse(findFliter)).ToList();
            client = null;

            if (query.Count > 0)
            {
                var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
                JArray JA = JArray.Parse(query.ToJson(jsonWriterSettings));
                foreach (JObject j in JA)
                {
                    j.Remove("_id");
                }
                return JA;
            }
            else { return new JArray(); }
        }

        public JArray GetDataPages(string mongodbConnStr, string mongodbDatabase, string coll, string sortStr, int pageCount, int pageNum, string findBson = "{}")
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);

            List<BsonDocument> query = collection.Find(BsonDocument.Parse(findBson)).Sort(sortStr).Skip(pageCount * (pageNum - 1)).Limit(pageCount).ToList();
            client = null;

            if (query.Count > 0)
            {

                var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
                JArray JA = JArray.Parse(query.ToJson(jsonWriterSettings));
                foreach (JObject j in JA)
                {
                    j.Remove("_id");
                }
                return JA;
            }
            else { return new JArray(); }
        }

        public JArray GetDataWithField(string mongodbConnStr, string mongodbDatabase, string coll, string fieldBson, string findBson = "{}")
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);

            List<BsonDocument> query = collection.Find(BsonDocument.Parse(findBson)).Project(BsonDocument.Parse(fieldBson)).ToList();
            client = null;

            if (query.Count > 0)
            {
                var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
                JArray JA = JArray.Parse(query.ToJson(jsonWriterSettings));
                foreach (JObject j in JA)
                {
                    j.Remove("_id");
                }
                return JA;
            }
            else { return new JArray(); }
        }

        public JArray GetDataPagesWithField(string mongodbConnStr, string mongodbDatabase, string coll, string fieldBson, int pageCount, int pageNum, string sortStr = "{}", string findBson = "{}")
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);

            List<BsonDocument> query = collection.Find(BsonDocument.Parse(findBson)).Project(BsonDocument.Parse(fieldBson)).Sort(sortStr).Skip(pageCount * (pageNum - 1)).Limit(pageCount).ToList();
            client = null;

            if (query.Count > 0)
            {
                var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
                JArray JA = JArray.Parse(query.ToJson(jsonWriterSettings));
                foreach (JObject j in JA)
                {
                    j.Remove("_id");
                }
                return JA;
            }
            else { return new JArray(); }
        }
        
        public List<T> GetData<T>(string mongodbConnStr, string mongodbDatabase, string coll, string findFliter = "{}", string sortFliter = "{}", int pageCount = 0, int pageNum = 0)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<T>(coll);

            List<T> query = null;
            if (pageCount == 0)
            {
                query = collection.Find(BsonDocument.Parse(findFliter)).ToList();
            }
            else
            {
                query = collection.Find(BsonDocument.Parse(findFliter)).Sort(sortFliter).Skip(pageCount * (pageNum - 1)).Limit(pageCount).ToList();
            }
            client = null;

            return query;
        }

        public List<T> GetDataWithField<T>(string mongodbConnStr, string mongodbDatabase, string coll, string fieldBson, string findFliter = "{}", string sortFliter = "{}", int pageCount = 0, int pageNum = 0)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<T>(coll);

            List<T> query = null;
            if (pageCount == 0)
            {
                query = collection.Find(BsonDocument.Parse(findFliter)).Project<T>(BsonDocument.Parse(fieldBson)).ToList();
            }
            else
            {
                query = collection.Find(BsonDocument.Parse(findFliter)).Project<T>(BsonDocument.Parse(fieldBson)).Sort(sortFliter).Skip(pageCount * (pageNum - 1)).Limit(pageCount).ToList();
            }
            client = null;

            return query;
        }

        
        public string InsertOneData(string mongodbConnStr, string mongodbDatabase, string coll, string insertBson)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);
            try
            {
                var query = collection.Find(BsonDocument.Parse(insertBson)).ToList();
                if (query.Count == 0)
                {
                    BsonDocument bson = BsonDocument.Parse(insertBson);
                    collection.InsertOne(bson);
                }
                client = null;
                return "suc";
            }
            catch (Exception e)
            {
                return e.ToString();
            }

        }

        public string InsertOneData(string mongodbConnStr, string mongodbDatabase, string coll, BsonDocument insertBson)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);
            try
            {
                collection.InsertOne(insertBson);
                client = null;
                return "suc";
            }
            catch (Exception e)
            {
                client = null;
                return "faild"; ;
            }

        }

        public string DeleteData(string mongodbConnStr, string mongodbDatabase, string coll, string deleteBson)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);
            try
            {
                var query = collection.Find(BsonDocument.Parse(deleteBson)).ToList();
                if (query.Count != 0)
                {
                    BsonDocument bson = BsonDocument.Parse(deleteBson);
                    collection.DeleteOne(bson);
                }
                client = null;
                return "suc";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public string ReplaceData(string mongodbConnStr, string mongodbDatabase, string collName, string whereFliter, string replaceFliter)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(collName);
            try
            {
                List<BsonDocument> query = collection.Find(whereFliter).ToList();
                if (query.Count > 0)
                {
                    collection.ReplaceOne(BsonDocument.Parse(whereFliter), BsonDocument.Parse(replaceFliter));
                    client = null;
                    return "suc";
                }
                else
                {
                    client = null;
                    return "no data";
                }
            }
            catch (Exception e)
            {
                client = null;
                return e.ToString();
            }
        }

        public string ReplaceOrInsertData(string mongodbConnStr, string mongodbDatabase, string collName, string whereFliter, string replaceFliter)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(collName);
            try
            {
                List<BsonDocument> query = collection.Find(whereFliter).ToList();
                if (query.Count > 0)
                {
                    collection.ReplaceOne(BsonDocument.Parse(whereFliter), BsonDocument.Parse(replaceFliter));
                    client = null;
                    return "suc";
                }
                else
                {
                    BsonDocument bson = BsonDocument.Parse(replaceFliter);
                    collection.InsertOne(bson);
                    client = null;
                    return "suc";
                }
            }
            catch (Exception e)
            {
                client = null;
                return e.ToString();
            }
        }

        public JArray GetDataAtBlock(string mongodbConnStr, string mongodbDatabase, string coll, string findFliter)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);

            List<BsonDocument> query = collection.Find(BsonDocument.Parse(findFliter)).ToList();
            client = null;

            if (query.Count > 0)
            {
                var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
                JArray JA = JArray.Parse(query.ToJson(jsonWriterSettings));
                foreach (JObject j in JA)
                {
                    j.Remove("_id");
                }
                return JA;
            }
            else { return new JArray(); }
        }

        public JArray Aggregate(string mongodbConnStr, string mongodbDatabase, string coll, string group)
        {
            var client = new MongoClient(mongodbConnStr);
            var database = client.GetDatabase(mongodbDatabase);
            var collection = database.GetCollection<BsonDocument>(coll);

            IList<IPipelineStageDefinition> stages = new List<IPipelineStageDefinition>();
            stages.Add(new JsonPipelineStageDefinition<BsonDocument, BsonDocument>(group));
            PipelineDefinition<BsonDocument, BsonDocument> pipeline = new PipelineStagePipelineDefinition<BsonDocument, BsonDocument>(stages);
            List<BsonDocument> query = collection.Aggregate(pipeline).ToList();
            client = null;
            if (query.Count > 0)
            {
                var jsonWriterSettings = new JsonWriterSettings { OutputMode = JsonOutputMode.Strict };
                JArray JA = JArray.Parse(query.ToJson(jsonWriterSettings));
                foreach (JObject j in JA)
                {
                    j.Remove("_id");
                }
                return JA;
            }
            else { return new JArray(); }
        }

    }
}

