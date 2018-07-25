using NEL_Scan_API.lib;
using Newtonsoft.Json.Linq;
using System;

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
