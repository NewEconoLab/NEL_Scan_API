using NEL_Scan_API.lib;
using Newtonsoft.Json.Linq;
using System;
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

        private string contractCallInfoCol = "contract_exec_detail";
        private string contractTxInfoCol = "Nep5Transfer";
        private string contractInfoCol = "contractCallState";


        public JArray getContractInfo(string hash)
        {
            var isNep5 = isNep5Asset(hash, out string assetName, out string assetSymbol);
            var res = new JObject{
                { "name",""},
                { "hash",hash},
                {"isNep5Asset", isNep5 },
                {"assetName", assetName },
                {"assetSymbol", assetSymbol },
                { "creator",""},
                { "createDate",1501234567},
                {"txCount", 0 },
                {"txCount24h", 0},
                {"usrCount", 0 },
                {"usrCount24h", 0 }
            };
            return new JArray { res };
        }
        public JArray getContractInfoOld(string hash)
        {
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
            var findJo = new JObject { { "contractHash", hash } };
            if (isOnly24h)
            {
                findJo.Add("blockIndex", new JObject { { "$gte", indexBefore24h } });
            }
            return mh.GetDataCount(Analysis_mongodbConnStr, Analysis_mongodbDatabase, "contract_call_info", findJo.ToString());
        }
        private long getUsrCount(string hash, bool isOnly24h = false, long indexBefore24h = 0)
        {
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

        public JArray getContractCallTx(string hash, int pageNum=1, int pageSize=10)
        {
            if (mh == null) return new JArray { };
            // txid + time + from + to + value(1neo,1gas) + fee
            var findStr = new JObject { { "type", ContractInvokeType.Call},{ "to", hash } }.ToString();
            var count = mh.GetDataCount(Block_mongodbConnStr, Block_mongodbDatabase, contractCallInfoCol, findStr);
            if(count == 0) return new JArray { new JObject { { "count", count }, { "list", new JArray() } } };

            var sortStr = new JObject { { "blockIndex", -1} }.ToString();
            var queryRes = mh.GetDataPages(Block_mongodbConnStr, Block_mongodbDatabase, contractCallInfoCol, sortStr, pageSize, pageNum, findStr);
            var res = queryRes.Select(p => {

                //var neoAmount = NumberDecimalHelper.formatDecimal(p["neoAmount"].ToString());
                //var gasAmount = NumberDecimalHelper.formatDecimal(p["gasAmount"].ToString());
                //var value = "";
                //if (neoAmount != "0") value += neoAmount + " NEO";
                //if (gasAmount != "0") value += ", " + gasAmount + " GAS";
                //if (value == "") value = "0";
                var value = "0";
                return new JObject {
                    { "txid", p["txid"]},
                    { "time", p["blockTimestamp"]},
                    { "from", p["from"]},
                    { "to", "当前合约"},
                    { "value", value},
                    { "net_fee", getNetFee(p["txid"].ToString()) + " CGAS"}
                };
            }).ToArray();

            return new JArray { new JObject {
                { "count", count},
                { "list", new JArray{ res } }
            } };
        }
        private string getNetFee(string txid)
        {
            var findStr = new JObject { { "txid", txid } }.ToString();
            var queryRes = mh.GetData(Block_mongodbConnStr, Block_mongodbDatabase, "tx", findStr);
            if (queryRes.Count == 0) return "0";

            var item = queryRes[0];
            var res = NumberDecimalHelper.formatDecimal(item["net_fee"].ToString());
            res = (decimal.Parse(res) / (new decimal(Math.Pow(10, 8)))).ToString();
            return res;
        }

        public JArray getContractNep5Tx(string address, int pageNum=1, int pageSize=10)
        {
            if(address.Length == 42 || address.Length == 40)
            {
                address = address.pubkeyhash2address();
            }
            var findStr = new JObject { { "$or", new JArray { new JObject { { "from", address} }, new JObject { { "to", address } } } } }.ToString();
            var count = mh.GetDataCount(Block_mongodbConnStr, Block_mongodbDatabase, contractTxInfoCol, findStr);
            if(count == 0) return new JArray { new JObject { { "count", count }, { "list", new JArray() } } };

            var sortStr = new JObject { { "blockindex", -1 } }.ToString();
            var queryRes = mh.GetDataPages(Block_mongodbConnStr, Block_mongodbDatabase, contractTxInfoCol, sortStr, pageSize, pageNum, findStr); ;
            if (queryRes.Count == 0) return new JArray { new JObject { { "count", count }, { "list", new JArray() } } };

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
                { "assetName", assetDict.GetValueOrDefault(p["asset"].ToString())},
            }).ToArray();

            

            return new JArray { new JObject {
                { "count", count},
                { "list", new JArray{ res } }
            } };
        }
        public JArray getContractNep5TxOld(string hash, int pageNum=1, int pageSize=10)
        {
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
                { "assetName", assetDict.GetValueOrDefault(p["asset"].ToString())},
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
            var queryRes = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "Nep5AssetInfo", fieldStr, findStr);
            return queryRes.ToDictionary(k => k["assetid"].ToString(), v => v["symbol"].ToString());
        }

        private bool isNep5Asset(string hash, out string name, out string symbol)
        {
            string findStr = new JObject { { "assetid", hash } }.ToString();
            var queryRes = mh.GetData(Block_mongodbConnStr, Block_mongodbDatabase, "Nep5AssetInfo", findStr);
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

    class ContractInvokeType
    {
        public const int Call = 1;
        public const int Create = 2;
        public const int Update = 3;
        public const int Destroy = 4;

    }

}
