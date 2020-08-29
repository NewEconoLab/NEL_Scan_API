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

        public string NEOPrice_mongodbConnStr { get; set; }
        public string NEOPrice_mongodbDatabase { get; set; }


        public JArray getScanTxCountHist()
        {
            System.Console.WriteLine();
            var findStr = "{}";
            var sortStr = "{'recordTime':-1}";
            var skip = 0;
            var limit = 30;
            var queryRes = mh.GetData(Block_mongodbConnStr, Block_mongodbDatabase, "txactiveinfos", findStr, sortStr, skip, limit);
            if (queryRes.Count == 0) return queryRes;

            var rr = queryRes.Select(p =>
            {
                var jo = new JObject();
                jo["count"] = p["count"];
                jo["time"] = p["recordTime"];
                return jo;
            }).ToArray();
            var res = new JObject {
                {"count", rr.Count() },
                {"list", new JArray{rr} }
            };
            return new JArray { res };

        }
        public JArray getScanStatistic()
        {
            var res = new JArray { new JObject {
                { "gasPrice", getPrice(gasPair)},
                { "neoPrice", getPrice(neoPair)},
                { "activeAddrCount", getActiveAddrCount()},
                { "gasAddrCount", getAddrCount(gasHash) },
                { "neoAddrCount", getAddrCount(neoHash) },
                { "txCount", getTxCount()}
            }};
            return res;
        }
        //
        private string gasPair = "GAS-USDT";
        private string neoPair = "NEO-USDT";
        private string getPrice(string pair)
        {
            var findStr = new JObject { { "instrument_id", pair } }.ToString();
            var sortStr = "{'time':-1}";
            var skip = 0;
            var limit = 1;
            var queryRes = mh.GetData(NEOPrice_mongodbConnStr, NEOPrice_mongodbDatabase, "priceinfos", findStr, sortStr, skip, limit);
            if (queryRes.Count() == 0) return "-1";

            var item = queryRes[0];
            return item["last"].ToString();
        }
        //
        private long SevenDaySeconds = 7 * 24 * 60 * 60;
        private long getActiveAddrCount()
        {
            var now = TimeHelper.GetTimeStamp();
            var stIndex = getBlockIndex(now - SevenDaySeconds);
            var edIndex = getBlockIndex(now);
            var findStr = new JObject { { "lastuse.blockindex", new JObject {
                { "$gte", stIndex},
                { "$lte", edIndex}
            } } }.ToString();
            var count = mh.GetDataCount(Block_mongodbConnStr, Block_mongodbDatabase, "address", findStr);
            return count;
        }
        private long getBlockIndex(long blocktime)
        {
            var findStr = new JObject { { "time", new JObject { { "$lte", blocktime*1000 } } } }.ToString();
            var sortStr = "{'index':-1}";
            var skip = 0;
            var limit = 1;
            var queryRes = mh.GetData(Block_mongodbConnStr, Block_mongodbDatabase, "block", findStr, sortStr, skip, limit);
            if (queryRes.Count == 0) return 5569626;

            var item = queryRes[0];
            return long.Parse(item["index"].ToString());
        }
        //
        //private string gasHash = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";
        //private string neoHash = "0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b";
        private string gasHash = "0x668e0c1f9d7b70a99dd9e06eadd4c784d641afbc";
        private string neoHash = "0xde5f57d430d3dece511cf975a8d37848cb9e0525";

        private long getAddrCount(string hash)
        {
            var findStr = new JObject { { "AssetHash", hash }, { "Balance", new JObject { { "$gt", 0 } } } }.ToString();
            var count = mh.GetDataCount(Block_mongodbConnStr, Block_mongodbDatabase, "Nep5State", findStr);
            return count;
        }
        //
        private long getTxCount()
        {
            var findStr = "{}";
            var count = mh.GetDataCount(Block_mongodbConnStr, Block_mongodbDatabase, "tx", findStr);
            return count;
        }


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
                //jo["value"] = double.Parse((string)jo["value"]) / System.Math.Pow(10,double.Parse((string)jo["decimals"]));
                jo["value"] = (decimal.Parse((string)jo["value"]) / new decimal(System.Math.Pow(10,double.Parse((string)jo["decimals"])))).ToString();
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
