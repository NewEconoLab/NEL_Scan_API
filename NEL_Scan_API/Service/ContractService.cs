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
        private long seconds24h = 24 * 60 * 60;
        private long getTimeBefore24h()
        {
            long time = TimeHelper.GetTimeStamp();
            time -= seconds24h;
            time *= 1000;
            return time;
        }

        //
        public JArray getContractNotify(string contractHash)
        {
            if (!contractHash.StartsWith("0x")) contractHash = "0x" + contractHash;

            return null;
        }
        //
        public JArray getContractManifest(string contractHash)
        {
            if (!contractHash.StartsWith("0x")) contractHash = "0x" + contractHash;
            var findStr = new JObject { { "contractHash", contractHash } }.ToString();

            var res = mh.GetData(Block_mongodbConnStr, Block_mongodbDatabase, "contract", findStr);
            if (res.Count == 0) return null;

            var manifest = res[0]["manifest"];
            return new JArray { new JObject { { "manifest", manifest } } };
        }

        // 合约列表
        public JArray getContractList(int pageNum = 1, int pageSize = 10)
        {
            var findStr = new JObject {
                {"$or", new JArray{
                     new JObject{ { "type", ContractInvokeType.Create } },
                     new JObject{{"type", ContractInvokeType .Update} }
                } }
            }.ToString();
            var sortStr = new JObject { { "time", -1 } }.ToString();
            var skip = (pageNum - 1) * pageSize;
            var limit = pageSize * 2;

            var count = mh.GetDataCount(Block_mongodbConnStr, Block_mongodbDatabase, "contract_exec_detail", findStr);
            var queryRes = mh.GetData(Block_mongodbConnStr, Block_mongodbDatabase, "contract_exec_detail", findStr, sortStr, skip, limit);
            if (queryRes.Count == 0) return queryRes;

            var res = queryRes.Select(p => formatAssetInfo(p))/*.Where(p => p["name"].ToString() != "")*/.Take(pageSize).ToArray();

            var rs = new JObject {
                {"count", count },
                {"list", new JArray{res } }
            };
            return new JArray { rs };
        }
        private JObject formatAssetInfo(JToken jt)
        {
            var res = new JObject();
            res["contractHash"] = jt["to"];
            res["deployTime"] = long.Parse(jt["blockTimestamp"].ToString())/1000;
            var author = getAssetAuthor(jt["to"].ToString(), out string name);
            res["name"] = name;
            res["author"] = author;
            return res;
        }
        private string getAssetAuthor(string assetid, out string assetName)
        {
            assetName = "";
            var findStr = new JObject { { "hash", assetid } }.ToString();
            var fieldStr = new JObject { { "script", 0 } }.ToString();
            //var queryRes = mh.GetDataWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, "contractCallState", fieldStr, findStr);
            //if (queryRes.Count == 0) return "";

            //var item = queryRes[0];
            //assetName = item["name"].ToString();
            //return item["author"].ToString();
            return "";
        }

        // 合约信息
        public JArray getContractInfo(string hash)
        {
            var isNep5 = isNep5Asset(hash, out string assetName, out string assetSymbol);
            long time = getTimeBefore24h();
            var res = new JObject{
                { "name",""},
                { "hash",hash},
                {"isNep5Asset", isNep5 },
                {"assetName", assetName },
                {"assetSymbol", assetSymbol },
                { "creator",""},
                { "createDate",getCreateTime(hash)},
                {"txCount", getTxCount(hash) },
                {"txCount24h", getTxCount(hash,true, time)},
                {"usrCount", getUsrCount(hash) },
                {"usrCount24h", getUsrCount(hash,true, time)},
            };
            return new JArray { res };
        }
        private long getCreateTime(string hash)
        {
            var findStr = new JObject { { "type", ContractInvokeType.Create }, { "to", hash } }.ToString();
            var queryRes = mh.GetData(Block_mongodbConnStr, Block_mongodbDatabase, contractCallInfoCol, findStr);
            if (queryRes.Count == 0) return 1501234567;

            var timeStr = queryRes[0]["blockTimestamp"].ToString();
            return long.Parse(timeStr) / 1000;
        }
        private long getTxCount(string hash, bool isOnly24h = false, long timeBefore24h = 0)
        {
            //var findJo = new JObject { { "to", hash }, { "level", 0} };
            var findJo = new JObject { { "level", 0 } };
            var hashArr = getContractHashArr(hash);
            var findArr = hashArr.Distinct().Select(p => new JObject { { "to", p} }).ToArray();
            findJo.Add("$or", new JArray { findArr });

            if (isOnly24h)
            {
                findJo.Add("blockTimestamp", new JObject { { "$gte", timeBefore24h } });
            }
            return mh.GetDataCount(Block_mongodbConnStr, Block_mongodbDatabase, "contract_exec_detail", findJo.ToString());
        }
        private long getUsrCount(string hash, bool isOnly24h = false, long timeBefore24h = 0)
        {
            //var findJo = new JObject { { "to", hash }, { "level", 0 } };
            var findJo = new JObject { { "level", 0 } };
            var hashArr = getContractHashArr(hash);
            var findArr = hashArr.Distinct().Select(p => new JObject { { "to", p } }).ToArray();
            findJo.Add("$or", new JArray { findArr });

            if (isOnly24h)
            {
                findJo.Add("blockTimestamp", new JObject { { "$gte", timeBefore24h } });
            }
            var list = new List<string>();
            list.Add(new JObject { { "$match", findJo } }.ToString());
            list.Add(new JObject { { "$group", new JObject { { "_id", "$from" }, { "sum", new JObject { { "$sum", 1 } } } } } }.ToString());
            list.Add(new JObject { { "$group", new JObject { { "_id", "$id" }, { "sum", new JObject { { "$sum", 1 } } } } } }.ToString());
            return mh.AggregateCount(Block_mongodbConnStr, Block_mongodbDatabase, "contract_exec_detail", list, false);
        }

        // 合约调用交易
        public JArray getContractCallTx(string hash, int pageNum=1, int pageSize=10)
        {
            if (mh == null) return new JArray { };
            // txid + time + from + to + value(1neo,1gas) + fee
            //var findStr = new JObject { { "type", ContractInvokeType.Call},{ "to", hash } }.ToString();
            var findJo = new JObject { { "type", ContractInvokeType.Call } };
            var hashArr = getContractHashArr(hash);
            var findArr = hashArr.Distinct().Select(p => new JObject { { "to", p } }).ToArray();
            findJo.Add("$or", new JArray { findArr });
            var findStr = findJo.ToString();

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
            var res = NumberDecimalHelper.formatDecimal(item["netfee"].ToString());
            res = (decimal.Parse(res) / (new decimal(Math.Pow(10, 8)))).ToString();
            return res;
        }

        // 合约nep5交易
        public JArray getContractNep5Tx(string address, int pageNum=1, int pageSize=10)
        {
            if(address.Length == 42 || address.Length == 40)
            {
                //address = address.pubkeyhash2address(); 
                //address = address.address2pubkeyHashN();
            }
            var findStr = new JObject { { "$or", new JArray { new JObject { { "from", address} }, new JObject { { "to", address } } } } }.ToString();
            var count = mh.GetDataCount(Block_mongodbConnStr, Block_mongodbDatabase, contractTxInfoCol, findStr);
            if(count == 0) return new JArray { new JObject { { "count", count }, { "list", new JArray() } } };

            var sortStr = new JObject { { "blockindex", -1 } }.ToString();
            var queryRes = mh.GetDataPages(Block_mongodbConnStr, Block_mongodbDatabase, contractTxInfoCol, sortStr, pageSize, pageNum, findStr); 
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

        public JArray getInnerTxAtContractDetail(string hash, int pageNum = 1, int pageSize = 10)
        {
            var hashArr = getContractHashArr(hash);
            var findArr = hashArr.SelectMany(p =>
            {
                return new JObject[] {
                    new JObject{{"from", p},{ "level", new JObject { { "$ne", 0 } }} },
                    new JObject{{"to", p},{ "level", new JObject { { "$ne", 0 } }} },
                    new JObject{{"to", p}, {"type", ContractInvokeType.Update}},
                    new JObject{{"to", p }, {"type", ContractInvokeType.Destroy}},
                    new JObject{{"to", p }, {"type", ContractInvokeType.Create}},
                };
            });

            var findStr = new JObject { { "$or", new JArray { findArr } } }.ToString();
            var count = mh.GetDataCount(Block_mongodbConnStr, Block_mongodbDatabase, "contract_exec_detail", findStr);
            if (count == 0) return new JArray { new JObject { { "count", count }, { "list", new JArray() } } };

            var sortStr = new JObject { { "blockIndex", -1 } }.ToString();
            var queryRes = mh.GetDataPages(Block_mongodbConnStr, Block_mongodbDatabase, "contract_exec_detail", sortStr, pageSize, pageNum, findStr);
            if (queryRes.Count == 0) return new JArray { new JObject { { "count", count }, { "list", new JArray() } } };

            //
            var indexArr = queryRes.Select(p => long.Parse(p["blockIndex"].ToString())).Distinct().ToArray();
            //var timeDict = getBlockTime(indexArr);

            var iRes =
            queryRes.Select(p =>
            {
                return new JObject {
                    {"txid", p["txid"]},
                    //{"time", timeDict.GetValueOrDefault(long.Parse(p["blockIndex"].ToString()),-1)},
                    {"time", long.Parse(p["blockTimestamp"].ToString())/1000 },
                    {"type", p["type"]},
                    {"from", p["from"]},
                    {"to", p["to"]}
                };
            }).ToArray();
            var res = new JArray { iRes };
            return new JArray { { new JObject { { "count", count }, { "list", res } } } };
        }

        private string[] getContractHashArr(string hash)
        {
            var contractId = getContractIdByHash(hash);
            if (contractId == "") return new string[] { hash};

            var contractHashArr = getContractHashById(contractId);
            if (contractHashArr == null || contractHashArr.Length == 0) return new string[] { hash };
            return contractHashArr;
        }
        private string getContractIdByHash(string hash)
        {
            var findStr = new JObject { { "contractHash", hash } }.ToString();
            var queryRes = mh.GetData(Block_mongodbConnStr, Block_mongodbDatabase, "contract", findStr);
            if (queryRes.Count == 0) return "";

            var res = queryRes[0]["contractId"].ToString();
            return res;

        }
        private string[] getContractHashById(string id)
        {
            var findStr = new JObject { { "contractId", long.Parse(id) } }.ToString();
            var queryRes = mh.GetData(Block_mongodbConnStr, Block_mongodbDatabase, "contract", findStr);
            if (queryRes.Count == 0) return null;

            var res = queryRes.Select(p => p["contractHash"].ToString()).Distinct().ToArray();
            return res;
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
