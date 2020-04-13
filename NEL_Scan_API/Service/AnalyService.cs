using NEL_Scan_API.lib;
using NEL_Scan_API.Service.constant;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace NEL_Scan_API.Service
{
    public class AnalyService
    {
        public mongoHelper mh { get; set; }
        public string block_mongodbConnStr { set; get; }
        public string block_mongodbDatabase { set; get; }
        public string analy_mongodbConnStr { set; get; }
        public string analy_mongodbDatabase { set; get; }

        //public JArray getAddressTxsNew(string address, int pageNum, int pageSize)//****************************************
        public JArray getAddressTxsNew2(string address, int pageSize, int pageNum)
        {
            var findStr = new JObject { { "$or", new JArray{
                new JObject{{"vinout.address", address}},
                new JObject{{"vout.address", address}}
            } }}.ToString();

            var count = mh.GetDataCount(block_mongodbConnStr, block_mongodbDatabase, "txdetail", findStr);
            if (count == 0)
            {
                return new JArray
                {
                    new JObject(){{"count", count }, { "list", new JArray() } }
                };
            }
            var sortStr = new JObject() { { "blockindex", -1 } }.ToString();
            var queryRes = mh.GetDataPages(block_mongodbConnStr, block_mongodbDatabase, "txdetail", sortStr, pageSize, pageNum, findStr);
            if (queryRes.Count == 0)
            {
                return new JArray
                {
                    new JObject(){{"count", count }, { "list", queryRes } }
                };
            }

            var rr = queryRes.Select(p =>
            {
                var jo = new JObject();
                jo["type"] = p["type"];
                jo["txid"] = p["txid"];
                jo["blockindex"] = p["blockindex"];
                jo["size"] = p["size"];
                jo["vinout"] = p["vinout"];
                jo["vout"] = p["vout"];
                jo["sys_fee"] = p["sys_fee"];
                jo["net_fee"] = p["net_fee"];
                jo["blocktime"] = p["blocktime"];
                return jo;
            }).ToArray();


            //
            var res = formatAssetName(new JArray { rr });

            return new JArray
            {
                new JObject(){{"count", count }, { "list", res } }
            };
        }
        private JArray formatAssetName(JArray queryRes)
        {
            var res =
            queryRes.Select(p =>
            {
                var ins = (JArray)p["vinout"];
                if (ins.Count > 0)
                {
                    var tp = ins.Select(pt =>
                    {
                        pt["assetName"] = AssetConst.getAssetName(pt["asset"].ToString());
                        return pt;
                    }).GroupBy(pg => pg["address"].ToString(), (kg, gg) => {
                        var address = kg;
                        var arr =
                        gg.GroupBy(pgg => pgg["asset"].ToString(), (kgg, ggg) =>
                        {
                            var val = ggg.Sum(p3g => decimal.Parse(p3g["value"].ToString()));
                            var unit = ggg.ToArray()[0]["assetName"].ToString();
                            return val + " " + unit;
                        }).ToArray();
                        var assetJA = new JArray { arr };
                        var tgRes = new JObject {
                            { "address", address},
                            { "assetJA", assetJA}
                        };
                        return tgRes;
                    });

                    p["vinout"] = new JArray { tp };
                }
                var outs = (JArray)p["vout"];
                if (outs.Count > 0)
                {
                    var tp = outs.Select(pt =>
                    {
                        pt["assetName"] = AssetConst.getAssetName(pt["asset"].ToString());
                        return pt;
                    }).GroupBy(pg => pg["address"].ToString(), (kg, gg) => {
                        var address = kg;
                        var arr =
                        gg.GroupBy(pgg => pgg["asset"].ToString(), (kgg, ggg) =>
                        {
                            var val = ggg.Sum(p3g => decimal.Parse(p3g["value"].ToString()));
                            var unit = ggg.ToArray()[0]["assetName"].ToString();
                            return val + " " + unit;
                        }).ToArray();
                        var assetJA = new JArray { arr };
                        var tgRes = new JObject {
                            { "address", address},
                            { "assetJA", assetJA}
                        };
                        return tgRes;
                    });
                    p["vout"] = new JArray { tp };
                }
                p["net_fee"] += " GAS";
                p["sys_fee"] += " GAS";
                var pj = (JObject)p;
                pj.Remove("vin");
                return pj;
            });
            return new JArray { res };
        }
        public JArray getAddressTxsNew(string address, int pageSize, int pageNum)
        {
            bool flag = true; if(flag) { return getAddressTxsNew2(address, pageSize, pageNum); }

            string findBson = "{'addr':'" + address + "'}";
            string sortStr = "{'blockindex' : -1}";
            JArray addrTxRes = mh.GetDataPages(block_mongodbConnStr, block_mongodbDatabase, "address_tx", sortStr, pageSize, pageNum, findBson);
            if (addrTxRes == null || addrTxRes.Count == 0)
            {
                return null;
            }
            string[] txidArr = addrTxRes.Select(p => p["txid"].ToString()).ToArray();
            findBson = MongoFieldHelper.toFilter(txidArr, "txid").ToString();
            JArray txRes = mh.GetData(block_mongodbConnStr, block_mongodbDatabase, "tx", findBson);
            if (txRes == null || txRes.Count == 0)
            {
                return null;
            }
            var txResNew = txRes.Where(p =>
            {
                if (p["vin"] == null)
                {
                    return false;
                }
                JArray ja = (JArray)p["vin"];
                if (ja == null || ja.Count == 0)
                {
                    return false;
                }
                return true;
            }).ToArray();

            Dictionary<string, JArray> txidVinOutDict = null;
            if (txResNew != null && txResNew.Count() > 0)
            {
                int[] vinIndex = txResNew.SelectMany(p => p["vin"].Select(pk => (int)pk["vout"])).ToArray();
                string[] vinTxid = txResNew.SelectMany(p => p["vin"].Select(pk => pk["txid"].ToString())).ToArray();
                findBson = MongoFieldHelper.toFilter(vinTxid, "txid").ToString();
                JArray txVinRes = mh.GetData(block_mongodbConnStr, block_mongodbDatabase, "tx", findBson);
                if (txVinRes != null && txVinRes.Count > 0)
                {
                    txidVinOutDict = txVinRes.ToDictionary(k => k["txid"].ToString(), v => (JArray)v["vout"]);
                }
            }
            

            Dictionary<string, JArray> txidVinOutIndexDict = txRes.ToDictionary(k => k["txid"].ToString(), v => {
                if(txidVinOutDict == null || txidVinOutDict.Count == 0)
                {
                    return new JArray();
                }
                JArray vin = (JArray)v["vin"];
                JArray vinOut = new JArray() {
                    vin.Select(p => (JObject)(txidVinOutDict.GetValueOrDefault(p["txid"].ToString())[(int)p["vout"]]))
                };
                return vinOut;
            });
            Dictionary<string, JArray> txidVoutDict = txRes.ToDictionary(k => k["txid"].ToString(), v => (JArray)v["vout"]);
            Dictionary<string, string> txidTypeDict = txRes.ToDictionary(k => k["txid"].ToString(), v => v["type"].ToString());
            foreach (JObject jo in addrTxRes)
            {
                string txid = jo["txid"].ToString();
                jo.Add("vin", txidVinOutIndexDict.GetValueOrDefault(txid));
                jo.Add("vout", txidVoutDict.GetValueOrDefault(txid));
                jo.Add("type", txidTypeDict.GetValueOrDefault(txid));
            }
            return new JArray() { new JObject() { { "count", addrTxRes.Count }, { "list", addrTxRes } } };
        }
        
        public JArray getRankByAsset(string asset, int pageSize, int pageNum, string network="testnet")
        {
            //if (network != "testnet") return getRankByAssetOld(asset, pageSize, pageNum);

            JObject filter = new JObject() { { "AssetHash", asset } };
            JObject sort = new JObject() { { "Balance", -1 } };
            JArray res = mh.GetDataPages(analy_mongodbConnStr, analy_mongodbDatabase, "address_assetid_balance", sort.ToString(), pageSize, pageNum, filter.ToString());
            for (var i = 0;i<res.Count;i++)
            {
                JObject jo = (JObject)res[i];
                res[i] = new JObject() { { "asset", (string)jo["AssetHash"] }, { "balance",jo["Balance"]["$numberDecimal"] } ,{ "addr",jo["Address"]} };
            }
            return res;
        }
        public JArray getRankByAssetCount(string asset, string network = "testnet")
        {
            //if (network != "testnet") return getRankByAssetCountOld(asset);

            JObject filter = new JObject() { { "AssetHash", asset } };
            long res = mh.GetDataCount(analy_mongodbConnStr, analy_mongodbDatabase, "address_assetid_balance", filter.ToString());
            
            return getJAbyKV("count", res);
        }

        private JArray getJAbyKV(string key, object value)
        {
            return new JArray { new JObject { { key, value.ToString() } } };
        }
        public JArray getRankByAssetOld(string asset, int pageSize, int pageNum)
        {
            JObject filter = new JObject() { { "asset", asset } };
            JObject sort = new JObject() { { "balance", -1 } };
            JArray res = mh.GetDataPages(analy_mongodbConnStr, analy_mongodbDatabase, "allAssetRank", sort.ToString(), pageSize, pageNum, filter.ToString());
            return res;
        }
        public JArray getRankByAssetCountOld(string asset)
        {
            JObject filter = new JObject() { { "asset", asset } };
            long res = mh.GetDataCount(analy_mongodbConnStr, analy_mongodbDatabase, "allAssetRank", filter.ToString());
            return getJAbyKV("count", res);
        }

    }
}
