using NEL_Scan_API.lib;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace NEL_Scan_API.Service
{
    public class ContractService
    {
        public mongoHelper mh { get; set; }
        public string Block_mongodbConnStr { get; set; }
        public string Block_mongodbDatabase { get; set; }
        public string Notify_mongodbConnStr { get; set; }
        public string Notify_mongodbDatabase { get; set; }
        public string Analysis_mongodbConnStr { get; set; }
        public string Analysis_mongodbDatabase { get; set; }

        private string contractCallInfoCol = "contract_call_info";
        private string contractTxInfoCol = "NEP5transfer";
        private string contractInfoCol = "contractCallState";

        private string getContractOriginHash(string hash)
        {
            var findStr = new JObject { { "updateBeforeHash", hash } }.ToString();
            var queryRes = mh.GetData(Analysis_mongodbConnStr, Analysis_mongodbDatabase, "contract_update_info", findStr);
            if (queryRes.Count == 0) return hash;

            return queryRes[0]["hash"].ToString();
        }
        private string getContractOriginHash2(string hash)
        {
            var findStr = new JObject { { "hash", hash } }.ToString();
            var queryRes = mh.GetData(Analysis_mongodbConnStr, Analysis_mongodbDatabase, "contract_update_info", findStr);
            if (queryRes.Count == 0) return hash;

            return queryRes[0]["updateBeforeHash"].ToString();
        }
        public JArray getContractInfo(string hash)
        {
            hash = hash.StartsWith("0x") ? hash: "0x"+hash;
            hash = getContractOriginHash2(hash);
            if (mh == null) return new JArray { };
            string findStr = new JObject { { "hash", hash} }.ToString();
            string fieldStr = new JObject { { "script", 0 } }.ToString();
            var queryRes = mh.GetDataWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, contractInfoCol, fieldStr, findStr);
            if (queryRes == null || queryRes.Count == 0) return new JArray { };

            var isNep5 = isNep5Asset(hash, out string assetName, out string assetSymbol);
            var beforIndex = getBlockHeightBefore24h();
            var p = queryRes[0];
            var res = new JObject {
                    {"name", p["name"] },
                    {"hash", p["hash"] },
                    {"isNep5Asset", isNep5 },
                    {"assetName", assetName },
                    {"assetSymbol", assetSymbol },
                    {"author", p["author"] },
                    {"email", p["email"] },
                    {"createDate", p["createDate"].ToString() == "0" ? 1501234567:p["createDate"]},
                    {"version", p["code_version"] },
                    {"description", p["description"] },
                    {"txCount", getTxCount(hash) },//p["txCount"] },
                    {"txCount24h", getTxCount(hash,true, beforIndex)},//p["txCount24h"] },
                    {"usrCount", getUsrCount(hash) },//p["usrCount"]},
                    {"usrCount24h", getUsrCount(hash,true, beforIndex)},//p["usrCount24h"] },
                };
            return new JArray { res };
        }

        private long getTxCount(string hash, bool isOnly24h = false, long indexBefore24h = 0)
        {
            hash = hash.StartsWith("0x") ? hash : "0x" + hash;
            hash = getContractOriginHash(hash);
            var findJo = new JObject { { "contractHash", hash } };
            if (isOnly24h)
            {
                findJo.Add("blockIndex", new JObject { { "$gte", indexBefore24h } });
            }
            return mh.GetDataCount(Analysis_mongodbConnStr, Analysis_mongodbDatabase, "contract_call_info", findJo.ToString());
        }
        private long getUsrCount(string hash, bool isOnly24h = false, long indexBefore24h = 0)
        {
            hash = hash.StartsWith("0x") ? hash : "0x" + hash;
            hash = getContractOriginHash(hash);
            var findJo = new JObject { { "contractHash", hash } };
            if (isOnly24h)
            {
                findJo.Add("blockIndex", new JObject { { "$gte", indexBefore24h } });
            }
            var list = new List<string>();
            list.Add(new JObject { { "$match", findJo } }.ToString());
            list.Add(new JObject { { "$group", new JObject { { "_id", "$address" }, { "sum", new JObject { { "$sum", 1 } } } } } }.ToString());
            list.Add(new JObject { { "$group", new JObject { { "_id", "$id" }, { "sum", new JObject { { "$sum", 1 } } } } } }.ToString());
            return mh.AggregateCount(Analysis_mongodbConnStr, Analysis_mongodbDatabase, "contract_call_info", list, false);
        }
        private long getBlockHeightBefore24h()
        {
            long now = TimeHelper.GetTimeStamp();
            long bfr = now - 24 * 60 * 60;
            string findStr = new JObject { { "time", new JObject { { "$gte", bfr } } } }.ToString();
            string sortStr = new JObject { { "time", 1 } }.ToString();
            string fieldStr = new JObject { { "index", 1 } }.ToString();
            var queryRes = mh.GetDataPagesWithField(Block_mongodbConnStr, Block_mongodbDatabase, "block", fieldStr, 1, 1, sortStr, findStr);
            if (queryRes == null || queryRes.Count == 0) return 0;
            return (long)queryRes[0]["index"];
        }

        private string[] getContractHashArr(string hash)
        {
            var findStr = new JObject { { "hash", hash }}.ToString();
            var queryRes = mh.GetData(Analysis_mongodbConnStr, Analysis_mongodbDatabase, "contract_update_info", findStr);
            if (queryRes.Count == 0)
            {
                return new string[] { hash };
            }
            return queryRes.SelectMany(p => new string[] { p["hash"].ToString(), p["updateBeforeHash"].ToString() }).Distinct().ToArray();
        }
        public JArray getContractCallTx(string hash, int pageNum=1, int pageSize=10)
        {
            if (mh == null) return new JArray { };
            // txid + time + from + to + value(1neo,1gas) + fee

            var hashArr = getContractHashArr(hash);
            var findStr = MongoFieldHelper.toFilter(hashArr, "contractHash").ToString();
            //string findStr = new JObject { { "contractHash", hash } }.ToString();
            string sortStr = new JObject { { "time", -1} }.ToString();
            var queryRes = mh.GetDataPages(Analysis_mongodbConnStr, Analysis_mongodbDatabase, contractCallInfoCol, sortStr, pageSize, pageNum, findStr);;
            if (queryRes == null || queryRes.Count == 0) return new JArray { };

            var res = queryRes.Select(p => {

                var neoAmount = NumberDecimalHelper.formatDecimal(p["neoAmount"].ToString());
                var gasAmount = NumberDecimalHelper.formatDecimal(p["gasAmount"].ToString());
                var value = "";
                if (neoAmount != "0") value += neoAmount + " NEO";
                if (gasAmount != "0")
                {
                    if (value != "") value += ", ";
                    value += gasAmount + " GAS";
                }
                if (value == "") value = "0";

                return new JObject {
                    { "txid", p["txid"]},
                    { "time", p["time"]},
                    { "from", p["address"]},
                    { "to", "当前合约"},
                    { "value", value},
                    { "net_fee", NumberDecimalHelper.formatDecimal(p["net_fee"].ToString()) + " GAS"}
                };
            }).ToArray();

            var count = mh.GetDataCount(Analysis_mongodbConnStr, Analysis_mongodbDatabase, contractCallInfoCol, findStr);
            
            return new JArray { new JObject {
                { "count", count},
                { "list", new JArray{ res } }
            } };
        }

        public JArray getContractNep5TxNew(string address, int pageNum=1, int pageSize=10)
        {
            if (mh == null) return new JArray { };
            string findStr = new JObject { { "$or", new JArray { new JObject { { "from", address} }, new JObject { { "to", address } } } } }.ToString();
            string sortStr = new JObject { { "time", -1 } }.ToString();
            var queryRes = mh.GetDataPages(Block_mongodbConnStr, Block_mongodbDatabase, contractTxInfoCol, sortStr, pageSize, pageNum, findStr); ;
            if (queryRes == null || queryRes.Count == 0) return new JArray { };

            var indexs = queryRes.Select(p => (long)p["blockindex"]).ToArray();
            var assets = queryRes.Select(p => p["asset"].ToString()).Distinct().ToArray();
            var txids = queryRes.Select(p => p["txid"].ToString()).Distinct().ToArray();
            var indexDict = getBlockTime(indexs);
            var assetDict = getAssetName(assets);
            var txidDict = getTxNetFee(txids);
            var res = queryRes.Select(p => new JObject {
                { "txid", p["txid"]},
                { "time", indexDict.GetValueOrDefault((long)p["blockindex"])},
                { "from", p["from"]},
                { "to", p["to"]},
                { "value", p["value"]},
                { "assetHash", p["asset"]},
                { "assetName", assetDict.GetValueOrDefault(p["asset"].ToString())},
                { "net_fee", txidDict.GetValueOrDefault(p["txid"].ToString()) + " GAS" }
            }).ToArray();

            var count = mh.GetDataCount(Block_mongodbConnStr, Block_mongodbDatabase, contractTxInfoCol, findStr);

            return new JArray { new JObject {
                { "count", count},
                { "list", new JArray{ res } }
            } };
        }
        public JArray getContractNep5Tx(string hash, int pageNum=1, int pageSize=10)
        {
            hash = hash.StartsWith("0x") ? hash : "0x" + hash;
            hash = getContractOriginHash(hash);
            bool flag = true;
            if (flag) return getContractNep5TxNew(hash, pageNum, pageSize);
            if (mh == null) return new JArray { };
            // txid + time + from + to + amount + assetName
            string findStr = new JObject { { "asset", hash } }.ToString();
            string sortStr = new JObject { { "time", -1 } }.ToString();
            var queryRes = mh.GetDataPages(Block_mongodbConnStr, Block_mongodbDatabase, contractTxInfoCol, sortStr, pageSize, pageNum, findStr); ;
            if (queryRes == null || queryRes.Count == 0) return new JArray { };

            var indexs = queryRes.Select(p => (long)p["blockindex"]).ToArray();
            var assets = queryRes.Select(p => p["asset"].ToString()).ToArray();
            var indexDict = getBlockTime(indexs);
            var assetDict = getAssetName(assets);
            var res = queryRes.Select(p => new JObject {
                { "txid", p["txid"]},
                { "time", indexDict.GetValueOrDefault((long)p["blockindex"])},
                { "from", p["from"]},
                { "to", p["to"]},
                { "value", p["value"]},
                { "assetHash", p["asset"]},
                { "assetName", assetDict.GetValueOrDefault(p["asset"].ToString())}
            }).ToArray();

            var count = mh.GetDataCount(Block_mongodbConnStr, Block_mongodbDatabase, contractTxInfoCol, findStr);

            return new JArray { new JObject {
                { "count", count},
                { "list", new JArray{ res } }
            } };
        }

        private Dictionary<long, long> getBlockTime(long[] indexs)
        {
            string findStr = MongoFieldHelper.toFilter(indexs, "index").ToString();
            string fieldStr = MongoFieldHelper.toReturn(new string[] { "index", "time"}).ToString();
            var queryRes = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "block", fieldStr, findStr);
            return queryRes.ToDictionary(k => (long)k["index"], v => (long)v["time"]);
        }
        private Dictionary<string, string> getAssetName(string[] assets)
        {
            string findStr = MongoFieldHelper.toFilter(assets, "assetid").ToString();
            string fieldStr = MongoFieldHelper.toReturn(new string[] { "assetid", "symbol" }).ToString();
            var queryRes = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "NEP5asset", fieldStr, findStr);
            return queryRes.ToDictionary(k => k["assetid"].ToString(), v => v["symbol"].ToString());
        }
        private Dictionary<string, string> getTxNetFee(string[] txids)
        {
            string findStr = MongoFieldHelper.toFilter(txids, "txid").ToString();
            string fieldStr = MongoFieldHelper.toReturn(new string[] { "txid", "net_fee" }).ToString();
            var queryRes = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "tx", fieldStr, findStr);
            return queryRes.ToDictionary(k => k["txid"].ToString(), v => v["net_fee"].ToString());
        }

        private bool isNep5Asset(string hash, out string name, out string symbol)
        {
            string findStr = new JObject { { "assetid", hash } }.ToString();
            var queryRes = mh.GetData(Block_mongodbConnStr, Block_mongodbDatabase, "NEP5asset", findStr);
            if(queryRes == null || queryRes.Count == 0)
            {
                name = "";
                symbol = "";
                return false;
            }
            name = queryRes[0]["name"].ToString();
            symbol = queryRes[0]["symbol"].ToString();
            return true;
        }
    }

}
