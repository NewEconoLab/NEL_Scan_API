using NEL_Scan_API.lib;
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
        public JArray getAddressTxsNew(string address, int pageSize, int pageNum)
        {
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
        
        public JArray getRankByAsset(string asset, int pageSize, int pageNum)
        {
            JObject filter = new JObject() { { "asset", asset } };
            JObject sort = new JObject() { { "balance", -1 } };
            JArray res = mh.GetDataPages(analy_mongodbConnStr, analy_mongodbDatabase, "allAssetRank", sort.ToString(), pageSize, pageNum, filter.ToString());
            return res;
        }
        public JArray getRankByAssetCount(string asset)
        {
            JObject filter = new JObject() { { "asset", asset } };
            long res = mh.GetDataCount(analy_mongodbConnStr, analy_mongodbDatabase, "allAssetRank", filter.ToString());
            return getJAbyKV("count", res);
        }

        private JArray getJAbyKV(string key, object value)
        {
            return new JArray { new JObject { { key, value.ToString() } } };
        }
    }
}
