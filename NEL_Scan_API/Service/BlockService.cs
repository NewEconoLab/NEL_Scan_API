using NEL_Scan_API.lib;
using NEL_Scan_API.Service.constant;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace NEL_Scan_API.Service
{
    public class BlockService
    {
        public mongoHelper mh { get; set; }
        public string Block_mongodbConnStr { get; set; }
        public string Block_mongodbDatabase { get; set; }
        public string Notify_mongodbConnStr { get; set; }
        public string Notify_mongodbDatabase { get; set; }

        public JArray getNep5Txlist(int pageNum =1, int pageSize=10)
        {
            return getNep5Tx("{}", pageNum, pageSize);
        }
        public JArray getNep5TxlistByAddress(string address, int pageNum = 1, int pageSize = 10)
        {
            string findStr = new JObject() { { "$or", new JArray { new JObject { { "from", address } }, new JObject { { "to", address } } } } }.ToString();
            return getNep5Tx(findStr, pageNum, pageSize);
        }
        private JArray getNep5Tx(string findStr, int pageNum, int pageSize)
        {
            string sortStr = new JObject { { "blockindex", -1 } }.ToString();
            var queryRes = mh.GetDataPages(Block_mongodbConnStr, Block_mongodbDatabase, "Nep5Transfer", sortStr, pageSize, pageNum, findStr);
            if (queryRes == null || queryRes.Count == 0) return new JArray { };


            // 转换区块时间
            long[] blockindexArr = queryRes.Select(p => long.Parse(p["blockindex"].ToString())).ToArray();
            var blockindexDict = getBlockTime(blockindexArr);
            
            // 转换资产名称
            string[] assetArr = queryRes.Select(p => p["asset"].ToString()).ToArray();
            var assetDict = getNep5AssetName(assetArr);
            var res = queryRes.Select(p => {
                JObject jo = (JObject)p;
                jo.Add("blocktime", blockindexDict.GetValueOrDefault(long.Parse(jo["blockindex"].ToString())));
                jo.Add("assetName", assetDict.GetValueOrDefault(jo["asset"].ToString()));
                if (jo["from"].ToString() == "")
                {
                    jo.Remove("from");
                    jo.Add("from", "system");
                }
                if (jo["to"].ToString() == "")
                {
                    jo.Remove("to");
                    jo.Add("to", "system");
                }
                jo["value"] = double.Parse((string)jo["value"]) / System.Math.Pow(10,double.Parse((string)jo["decimals"]));
                jo.Remove("blockindex");
                jo.Remove("decimals");
                jo.Remove("asset");
                
                jo.Remove("n");
                return jo;
            }).ToArray();

            long count = mh.GetDataCount(Block_mongodbConnStr, Block_mongodbDatabase, "Nep5Transfer", findStr);
            
            return new JArray { new JObject {
                { "count", count},
                { "list", new JArray{res} }
            } };
        }

        public JArray gettransactionlist(int pageNum=1, int pageSize=10, string type="")
        {
            string findStr = "{}";
            bool addType = type != "" && type != null && type != "all";
            if (addType)
            {
                findStr = new JObject() { { "type", type } }.ToString();
            }
            long count = mh.GetDataCount(Block_mongodbConnStr, Block_mongodbDatabase, "tx", findStr);
            string fieldStr = MongoFieldHelper.toReturn(new string[] {"sender", "txid", "blockindex", "size" }).ToString();
            string sortStr = new JObject() { { "blockindex", -1 } }.ToString();
            JArray query = mh.GetDataPagesWithField(Block_mongodbConnStr, Block_mongodbDatabase, "tx", fieldStr, pageSize, pageNum, sortStr, findStr);
            
            return new JArray
            {
                new JObject(){{"count", count }, { "list", query}}
            };
        }

        private long getBlockTime(long index)
        {
            string findStr = new JObject() { {"index", index } }.ToString();
            string fieldStr = new JObject() { {"time", 1 } }.ToString();
            var query = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "block", fieldStr, findStr);
            return long.Parse(query[0]["time"].ToString());
        }
        private Dictionary<long, long> getBlockTime(long[] indexs)
        {
            string findStr = MongoFieldHelper.toFilter(indexs.Distinct().ToArray(), "index").ToString();
            string fieldStr = new JObject() { { "index", 1 },{ "time", 1 } }.ToString();
            var query = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "block", fieldStr, findStr);
            return query.ToDictionary(k=>long.Parse(k["index"].ToString()), v=>long.Parse(v["time"].ToString()));
        }
        private Dictionary<string, string> getNep5AssetName(string[] assetIds)
        {
            string findStr = MongoFieldHelper.toFilter(assetIds.Distinct().ToArray(), "assetid").ToString();
            string fieldStr = new JObject() { { "assetid", 1 },{ "symbol", 1 }}.ToString();
            var query = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "Nep5AssetInfo", fieldStr, findStr);
            return query.ToDictionary(k=>k["assetid"].ToString(), v=>v["symbol"].ToString());
        }

        private Dictionary<string, string> getAssetName(string[] assetIds)
        {
            string findStr = MongoFieldHelper.toFilter(assetIds.Distinct().ToArray(), "id").ToString();
            string fieldStr = new JObject() { { "name.name", 1 }, { "id", 1 } }.ToString();
            var query = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "asset", fieldStr, findStr);
            var nameDict =
                query.ToDictionary(k => k["id"].ToString(), v =>
                {
                    string id = v["id"].ToString();
                    if (id == AssetConst.id_neo)
                    {
                        return AssetConst.id_neo_nick;
                    }
                    if (id == AssetConst.id_gas)
                    {
                        return AssetConst.id_gas_nick;
                    }
                    string name = v["name"][0]["name"].ToString();
                    return name;
                });
            return nameDict;
        }
       
        private JObject[] formatAssetName(JObject[] query, Dictionary<string, string> nameDict)
        {
            return query.Select(p =>
                   {
                       JObject jo = p;
                       string id = jo["asset"].ToString();
                       string idName = nameDict.GetValueOrDefault(id);
                       jo.Remove("asset");
                       jo.Add("asset", idName);
                       return jo;
                   }).ToArray();
        }

        private JArray formatAssetNameByIds(JArray query, string[] assetIds)
        {
            var nameDict = getAssetName(assetIds);
            return new JArray
                {
                    query.Select(p =>
                    {
                        JObject jo = (JObject)p;
                        string id = jo["asset"].ToString();
                        string idName = nameDict.GetValueOrDefault(id);
                        jo.Remove("asset");
                        jo.Add("asset", idName);
                        return jo;
                    }).ToArray()
                };
        }
        
    }
}
