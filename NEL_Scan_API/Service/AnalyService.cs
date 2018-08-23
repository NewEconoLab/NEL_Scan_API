using NEL_Scan_API.lib;
using Newtonsoft.Json.Linq;
using System;
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
        public string nelJsonRPCUrl { set; get; }

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
            int[] vinIndex = txRes.SelectMany(p => p["vin"].Select(pk => (int)pk["vout"])).ToArray();
            string[] vinTxid = txRes.SelectMany(p => p["vin"].Select(pk => pk["txid"].ToString())).ToArray();
            findBson = MongoFieldHelper.toFilter(vinTxid, "txid").ToString();
            JArray txVinRes = mh.GetData(block_mongodbConnStr, block_mongodbDatabase, "tx", findBson);
            if (txVinRes == null || txVinRes.Count == 0)
            {
                return null;
            }
            Dictionary<string, JArray> txidVinOutDict = txVinRes.ToDictionary(k => k["txid"].ToString(), v => (JArray)v["vout"]);
            Dictionary<string, JArray> txidVinOutIndexDict = txRes.ToDictionary(k => k["txid"].ToString(), v => {
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
            //return addrTxRes;
            return new JArray() { new JObject() { { "count", addrTxRes.Count }, { "list", addrTxRes } } };
        }
        
        public JArray getAddressTxs(string address, int pageSize, int pageNum)
        {
            JArray result = null;
            try
            {
                byte[] postdata;
                string url = httpHelper.MakeRpcUrlPost(nelJsonRPCUrl, "getaddresstxs", out postdata, new MyJson.JsonNode_ValueString(address), new MyJson.JsonNode_ValueNumber(pageSize), new MyJson.JsonNode_ValueNumber(pageNum));
                result = (JArray) JObject.Parse(httpHelper.HttpPost(url, postdata))["result"];
                
                foreach (JObject jo in result)
                {
                    url = httpHelper.MakeRpcUrlPost(nelJsonRPCUrl, "getrawtransaction", out postdata, new MyJson.JsonNode_ValueString(jo["txid"].ToString()));
                    JObject JOresult = (JObject)((JArray)JObject.Parse(httpHelper.HttpPost(url, postdata))["result"])[0];
                    string type = JOresult["type"].ToString();
                    jo.Add("type", type);
                    JArray Vout = (JArray)JOresult["vout"];
                    jo.Add("vout", Vout);
                    JArray _Vin = (JArray)JOresult["vin"];
                    JArray Vin = new JArray();
                    foreach (JObject vin in _Vin)
                    {
                        string txid = vin["txid"].ToString();
                        int n = (int)vin["vout"];
                        string filter = "{txid:'" + txid + "'}";
                        JObject JOresult2 = (JObject)mh.GetDataAtBlock(block_mongodbConnStr, block_mongodbDatabase, "tx", filter)[0];
                        Vin.Add((JObject)((JArray) JOresult2["vout"])[n]);
                    }
                    jo.Add("vin", Vin);
                    jo.Add("vin.Count", Vin.Count);
                }
                result.Add(new JObject { { "size", result.Count } });
            }
            catch (Exception e)
            {
                result = getJAbyKV("result", "errMsg:" + e.Message);
            }
            return result;
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
