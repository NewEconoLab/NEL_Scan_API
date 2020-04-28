using NEL_Scan_API.lib;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace NEL_Scan_API.Service
{
    public class InnerTxService
    {
        public mongoHelper mh { get; set; }
        public string block_mongodbConnStr { set; get; }
        public string block_mongodbDatabase { set; get; }
        public string analy_mongodbConnStr { set; get; }
        public string analy_mongodbDatabase { set; get; }
        public string innerTxColl { get; set; } = "contract_exec_detail";


        public JArray getInnerTxAtContractDetail(string hash, int pageNum = 1, int pageSize = 10)
        {
            var findStr = new JObject { { "$or", new JArray
            {
                new JObject{{"from", hash},{ "level", new JObject { { "$ne", 0 } }} },
                new JObject{{"to", hash},{ "level", new JObject { { "$ne", 0 } }} },
            } } }.ToString();
            var count = mh.GetDataCount(block_mongodbConnStr, block_mongodbDatabase, innerTxColl, findStr);
            if (count == 0) return new JArray { new JObject { { "count", count }, { "list", new JArray() } } };

            var sortStr = new JObject { { "blockIndex", -1 } }.ToString();
            var queryRes = mh.GetDataPages(block_mongodbConnStr, block_mongodbDatabase, innerTxColl, sortStr, pageSize, pageNum, findStr);
            if (queryRes.Count == 0) return new JArray { new JObject { { "count", count }, { "list", new JArray() } } };

            //
            var indexArr = queryRes.Select(p => long.Parse(p["blockIndex"].ToString())).Distinct().ToArray();
            var timeDict = getBlockTime(indexArr);

            var iRes =
            queryRes.Select(p =>
            {
                return new JObject {
                    {"txid", p["txid"]},
                    {"time", timeDict.GetValueOrDefault(long.Parse(p["blockIndex"].ToString()),-1)},
                    {"type", p["type"]},
                    {"from", p["from"]},
                    {"to", p["to"]}
                };
            }).ToArray();
            var res = new JArray { iRes };
            return new JArray { { new JObject { { "count", count }, { "list", res } } } };
        }


        public JArray getInnerTxAtAddrDetail(string address, int pageNum = 1, int pageSize = 10)
        {
            var pubkeyHash = address.address2pubkeyHash();
            var findStr = getInnerFindStr(pubkeyHash, pageNum, pageSize);
            if (findStr == null) return new JArray { new JObject { { "count", 0 }, { "list", new JArray() } } };

            var count = mh.GetDataCount(analy_mongodbConnStr, analy_mongodbDatabase, innerTxColl, findStr);
            if (count == 0) return new JArray { new JObject { { "count", count }, { "list", new JArray() } } };

            var sortStr = new JObject { { "index", -1 } }.ToString();
            var queryRes = mh.GetDataPages(analy_mongodbConnStr, analy_mongodbDatabase, innerTxColl, sortStr, pageSize, pageNum, findStr);
            if (queryRes.Count == 0) return new JArray { new JObject { { "count", count }, { "list", new JArray() } } };

            //
            var indexArr = queryRes.Select(p => long.Parse(p["blockIndex"].ToString())).Distinct().ToArray();
            var timeDict = getBlockTime(indexArr);

            var iRes =
            queryRes.Select(p =>
            {
                return new JObject {
                    {"txid", p["txid"]},
                    {"time", timeDict.GetValueOrDefault(long.Parse(p["blockIndex"].ToString()),-1)},
                    {"type", p["type"]},
                    {"from", p["from"]},
                    {"to", p["to"]}
                };
            }).ToArray();
            var res = new JArray { iRes };
            return new JArray { { new JObject { { "count", count }, { "list", res } } } };
        }
        private string getInnerFindStr(string pubkeyHash, int pageNum, int pageSize)
        {
            var findStr = new JObject { { "from", pubkeyHash } }.ToString();
            var count = mh.GetDataCount(analy_mongodbConnStr, analy_mongodbDatabase, innerTxColl, findStr);
            if (count == 0) return null;

            var sortStr = new JObject { { "index", -1 } }.ToString();
            var queryRes = mh.GetDataPages(analy_mongodbConnStr, analy_mongodbDatabase, innerTxColl, sortStr, pageSize, pageNum, findStr);
            if (queryRes.Count == 0) return null;

            var txidArr = queryRes.Select(p => p["txid"].ToString()).Distinct().ToArray();
            //
            var findJA = new JArray();
            foreach (var txid in txidArr)
            {
                findJA.Add(new JObject { { "txid", txid }, { "from", new JObject { { "$ne", pubkeyHash } } } });
            }
            var findJo = new JObject { { "$or", findJA } };
            return findJo.ToString();
        }


        public JArray getInnerTxAtTxDetail(string txid)
        {
            var findStr = new JObject { { "txid", txid } }.ToString();
            var queryRes = mh.GetData(analy_mongodbConnStr, analy_mongodbDatabase, innerTxColl, findStr);
            if (queryRes.Count == 0) return new JArray();
            //
            List<TxCallInfo> list = new List<TxCallInfo>();
            var iRes = queryRes.OrderBy(p => (int)p["index"]).ToArray();
            foreach (var item in iRes)
            {
                var txCallInfo = new TxCallInfo();
                txCallInfo.type = int.Parse(item["type"].ToString());
                txCallInfo.from = item["from"].ToString();
                txCallInfo.to = item["to"].ToString();
                txCallInfo.index = (int)item["index"];
                //
                var level = (int)item["level"];
                if (level == 0)
                {
                    txCallInfo.caller = txCallInfo.from.pubkeyhash2address();
                    txCallInfo.callee = txCallInfo.to;
                    txCallInfo.callIndex = txCallInfo.index;
                    txCallInfo.orderId = "";
                    txCallInfo.txNum = 0;

                    list.Add(txCallInfo);
                    continue;
                }
                var rr = list.Where(p => p.to.ToString() == txCallInfo.from).OrderByDescending(p => p.index).First();
                if (rr != null)
                {
                    txCallInfo.caller = rr.caller;
                    txCallInfo.callee = rr.callee;
                    txCallInfo.callIndex = rr.callIndex;
                    var id = rr.orderId;
                    if (id != "") id += "-";
                    txCallInfo.orderId = id + (++rr.txNum).ToString().PadLeft(2, '0');
                    txCallInfo.txNum = 0;
                }
                list.Add(txCallInfo);
            }
            //
            var jRes =
            list.GroupBy(p => p.caller + "_" + p.callee + "_" + p.callIndex/*new JObject {
                    {"caller", p.caller},
                    {"callee", p.callee}
                }*/, (k, g) =>
                   {

                       var ul = g.Select(pg =>
                       {
                           var jo = new JObject();
                           //jo["caller"] = pg.caller;
                           //jo["callee"] = pg.callee;
                           jo["orderId"] = pg.orderId;
                           //jo["txNum"] = p.txNum;
                           jo["type"] = pg.type;
                           jo["from"] = pg.from;
                           jo["to"] = pg.to;
                           //jo["index"] = p.index;
                           return jo;
                       }).Where(p => p["orderId"].ToString() != "").ToList();
                       var kk = k.Split("_");
                       var rr = new JObject();
                       rr["caller"] = kk[0];
                       rr["callee"] = kk[1];
                       rr["txCount"] = ul.Count;
                       rr["txList"] = new JArray { ul };
                       return rr;
                   }).ToList();
            var res = new JObject { { "count", jRes.Count }, { "list", new JArray { jRes } } };

            return new JArray { res };
        }
        class TxCallInfo
        {
            public string caller;
            public string callee;
            public int callIndex;
            public string orderId;
            public int txNum;//0
            public int type;
            public string from;
            public string to;
            public int index;
        }


        public JArray getInnerTxAtTxList(int pageNum = 1, int pageSize = 10)
        {
            var findStr = new JObject { { "level", new JObject { { "$ne", 0 } } } }.ToString();
            var count = mh.GetDataCount(analy_mongodbConnStr, analy_mongodbDatabase, innerTxColl, findStr);
            if (count == 0) return new JArray { { new JObject { { "count", count }, { "list", new JArray() } } } };

            var sortStr = new JObject { { "blockIndex", -1 } }.ToString();
            var queryRes = mh.GetDataPages(analy_mongodbConnStr, analy_mongodbDatabase, innerTxColl, sortStr, pageSize, pageNum, findStr);
            if (queryRes.Count == 0) return new JArray { { new JObject { { "count", count }, { "list", new JArray() } } } };

            //
            var indexArr = queryRes.Select(p => long.Parse(p["blockIndex"].ToString())).Distinct().ToArray();
            var timeDict = getBlockTime(indexArr);

            var iRes =
            queryRes.Select(p =>
            {
                return new JObject {
                    {"txid", p["txid"]},
                    {"time", timeDict.GetValueOrDefault(long.Parse(p["blockIndex"].ToString()),-1)},
                    {"type", p["type"]},
                    {"from", p["from"]},
                    {"to", p["to"]}
                };
            }).ToArray();
            var res = new JArray { iRes };
            return new JArray { { new JObject { { "count", count }, { "list", res } } } };
        }
        private Dictionary<long, long> getBlockTime(long[] indexs)
        {
            string findStr = MongoFieldHelper.toFilter(indexs.Distinct().ToArray(), "index").ToString();
            string fieldStr = new JObject() { { "index", 1 }, { "time", 1 } }.ToString();
            var query = mh.GetDataWithField(block_mongodbConnStr, block_mongodbDatabase, "block", fieldStr, findStr);
            return query.ToDictionary(k => long.Parse(k["index"].ToString()), v => long.Parse(v["time"].ToString()));
        }
    }
    enum InvokeType
    {
        Call = 1,
        Create = 2,
        Update = 3,
        Destory = 4
    }
}
