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
            return addrTxRes;
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
