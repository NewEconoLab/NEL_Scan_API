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
            return new JArray
            {
                new JObject(){{"count", addrTxRes.Count }, { "list", addrTxRes } }
            }; 
        }
        
        public JArray getRankByAssetOld(string asset, int pageSize, int pageNum, string network="testnet")
        {
            JObject filter = new JObject() { { "AssetHash", asset } };
            JObject sort = new JObject() { { "Balance", -1 } };
            JArray res = mh.GetDataPages(block_mongodbConnStr, block_mongodbDatabase, "Nep5State", sort.ToString(), pageSize, pageNum, filter.ToString());
            for (var i = 0;i<res.Count;i++)
            {
                JObject jo = (JObject)res[i];
                var balance = double.Parse((string)jo["Balance"]["$numberDecimal"]) / System.Math.Pow(10,double.Parse((string)jo["AssetDecimals"])); 
                res[i] = new JObject() { { "asset", (string)jo["AssetHash"] }, { "balance", balance } ,{ "addr",jo["Address"]} };
            }
            return res;
        }
        public JArray getRankByAsset(string asset, int pageSize, int pageNum, string network = "testnet")
        {
            var hashArr = getRelateHashArr(asset);
            var hashJOs = hashArr.Select(p => new JObject { { "AssetHash", p } }).ToArray();
            var findStr = new JObject { { "$or", new JArray { hashJOs } } }.ToString();
            var sortStr = new JObject() { { "_id", 1 } }.ToString();
            var queryRes = mh.GetDataPagesWithSkip(block_mongodbConnStr, block_mongodbDatabase, "Nep5State", sortStr, pageSize*(pageNum-1), pageSize*10, findStr);

            var cnt = 0;
            var ja = new JArray();
            foreach(var item in queryRes)
            {
                var address = item["Address"].ToString();
                var assetHash = item["AssetHash"].ToString();
                var arr = ja.Where(p => p["addr"].ToString() == address).ToArray();
                if (arr != null && arr.Length > 0)
                {
                    ja.Remove(arr[0]);
                }

                var balance = double.Parse((string)item["Balance"]["$numberDecimal"]) / System.Math.Pow(10, double.Parse((string)item["AssetDecimals"]));
                var newItem = new JObject {
                    { "asset", (string)item["AssetHash"] },
                    { "balance", balance },
                    { "addr", item["Address"] } };
                ja.Add(newItem);
                if(++cnt == pageSize)
                {
                    break;
                }
            }
            return ja;
        }
        private string[] getRelateHashArr(string asset)
        {
            var contractId = getContractId(asset);
            if (contractId == -1) return new string[] { asset };
            //
            var findStr = new JObject { { "contractId", contractId } }.ToString();
            var queryRes = mh.GetData(block_mongodbConnStr, block_mongodbDatabase, "contract", findStr);
            //
            var hashArr = queryRes.Select(p => p["contractHash"].ToString()).ToArray();
            return hashArr;

        }
        private long getContractId(string asset)
        {
            var findStr = new JObject { { "contractHash", asset } }.ToString();
            var queryRes = mh.GetData(block_mongodbConnStr, block_mongodbDatabase, "contract", findStr);
            if (queryRes.Count == 0) return -1;

            return long.Parse(queryRes[0]["contractId"].ToString());
        }

        public JArray getRankByAssetCount(string asset, string network = "testnet")
        {
            var hashArr = getRelateHashArr(asset);
            var hashJOs = hashArr.Select(p => new JObject { { "AssetHash", p } }).ToArray();
            var findStr = new JObject { { "$or", new JArray { hashJOs } } };

            var list = new List<string>();
            list.Add(new JObject { { "$match", findStr } }.ToString());
            list.Add(new JObject { { "$group", new JObject {
                { "_id", "$Address" }, { "sum", new JObject { { "$sum", 1 } } } } } }.ToString());
            var cnt = mh.AggregateCount(block_mongodbConnStr, block_mongodbDatabase, "Nep5State", list);
            return getJAbyKV("count", cnt);
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
