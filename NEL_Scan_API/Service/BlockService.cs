using NEL_Scan_API.lib;
using NEL_Scan_API.Service.constant;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NEL_Scan_API.Service
{
    public class BlockService
    {
        public mongoHelper mh { get; set; }
        public string Block_mongodbConnStr { get; set; }
        public string Block_mongodbDatabase { get; set; }
        public string Notify_mongodbConnStr { get; set; }
        public string Notify_mongodbDatabase { get; set; }

        public JArray getutxolistbyaddress(string address, int pageNum=1, int pageSize=10) {

            string findStr = new JObject() { { "addr", address } }.ToString();
            long count = mh.GetDataCount(Block_mongodbConnStr, Block_mongodbDatabase, "utxo", findStr);
            
            string fieldStr = MongoFieldHelper.toReturn(new string[] { "asset", "txid", "value"}).ToString();
            string sortStr = new JObject() { {"createHeight", -1 } }.ToString();
            JArray query = mh.GetDataPagesWithField(Block_mongodbConnStr, Block_mongodbDatabase, "utxo", fieldStr,  pageSize, pageNum, sortStr, findStr);

            // assetId --> assetName
            if(query != null && query.Count > 0)
            {
                string[] assetIds = query.Select(p => p["asset"].ToString()).Distinct().ToArray();
                query = formatAssetNameByIds(query, assetIds);
            }

            return new JArray
            {
                new JObject(){{"count", count }, { "list", query}}
            };
        }
        public JArray gettransactionlist(int pageNum=1, int pageSize=10, string type="all")
        {
            JObject findJo = new JObject() { { "blockindex", new JObject() { { "$gt", -1 } } } };
            if(type != "all")
            {
                findJo.Add("type", type);
            }
            string findStr = findJo.ToString();
            long count = mh.GetDataCount(Block_mongodbConnStr, Block_mongodbDatabase, "tx", findStr);
            string fieldStr = MongoFieldHelper.toReturn(new string[] {"type", "txid", "blockindex", "size" }).ToString();
            findStr = "{}";
            if(type != "all")
            {
                findStr = new JObject() { { "type", type } }.ToString();
            }
            string sortStr = new JObject() { { "blockindex", -1 } }.ToString();
            JArray query = mh.GetDataPagesWithField(Block_mongodbConnStr, Block_mongodbDatabase, "tx", fieldStr, pageSize, pageNum, sortStr, findStr);
            return new JArray
            {
                new JObject(){{"count", count }, { "list", query}}
            };
        }

        public JArray getutxoinfo(string txid)
        {
            string findStr = new JObject() { { "txid", txid } }.ToString();
            string fieldStr = MongoFieldHelper.toReturn(new string[] {"txid", "type","net_fee","sys_fee","size","blockindex","blocktime","vin","vout" }).ToString();
            var query = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "tx", fieldStr, findStr);
            if (query == null || query.Count == 0) return new JArray();

            var tx = (JObject)query[0];
            // 更新时间
            if (tx["blocktime"] == null)
            {
                long blockindex = long.Parse(tx["blockindex"].ToString());
                long blocktime = getBlockTime(blockindex);
                tx.Remove("blocktime");
                tx.Add("blocktime", blocktime);
            }

            List<string> assetIds = new List<string>();

            // 更新vin
            JObject[] vins = null;
            if (tx["vin"] != null && tx["vin"].ToString() != "[]")
            {
                vins = ((JArray)tx["vin"]).Select(p => {
                    JObject jo = (JObject)p;
                    string vintxid = jo["txid"].ToString();
                    string vinn = jo["vout"].ToString();
                    string subfindStr = new JObject() {{ "txid", vintxid} }.ToString();
                    string subfieldStr = new JObject() { { "vout", 1 } }.ToString();
                    var subquery = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "tx", subfieldStr, subfindStr);

                    var vinVouts = (JArray)subquery[0]["vout"];
                    var vinVout = (JObject)vinVouts.Where(ps => ps["n"].ToString() == vinn).ToArray()[0];
                    vinVout.Remove("n");
                    return vinVout;
                }).ToArray();
                
                assetIds.AddRange(vins.Select(p => p["asset"].ToString()).ToList().Distinct());
            }
            // 删除vout.n
            JObject[] vouts = null;
            if (tx["vout"] != null && tx["vout"].ToString() != "[]")
            {
                vouts = ((JArray)tx["vout"]).Select(p => {
                    JObject jo = (JObject)p;
                    jo.Remove("n");
                    return jo;
                }).ToArray();

                assetIds.AddRange(vouts.Select(p => p["asset"].ToString()).ToList().Distinct());
            }

            // assetId-->assetName
            if(assetIds.Count > 0 )
            {
                var nameDict = getAssetName(assetIds.Distinct().ToArray());
                if(vins != null)
                {
                    tx.Remove("vin");
                    tx.Add("vin", new JArray { formatAssetName(vins, nameDict).ToArray() });
                }
                if (vouts != null)
                {
                    tx.Remove("vout");
                    tx.Add("vout", new JArray { formatAssetName(vouts, nameDict).ToArray() });
                }
            }

            return new JArray { tx };
        }

        private long getBlockTime(long index)
        {
            string findStr = new JObject() { {"index", index } }.ToString();
            string fieldStr = new JObject() { {"time", 1 } }.ToString();
            var query = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "block", fieldStr, findStr);
            return long.Parse(query[0]["time"].ToString());
        }

        private Dictionary<string, string> getAssetName(string[] assetIds)
        {
            string findStr = MongoFieldHelper.toFilter(assetIds.Distinct().ToArray(), "id").ToString();
            string fieldStr = new JObject() { { "name.name", 1 }, { "id", 1 } }.ToString();
            var query = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "asset", fieldStr, findStr);
            var nameDict =
                query.ToDictionary(k => k["id"].ToString(), v =>
                {
                    string id = v["id"].ToString();
                    if (id == AssetConst.id_neo)
                    {
                        return AssetConst.id_neo_nick;
                    }
                    if (id == AssetConst.id_gas)
                    {
                        return AssetConst.id_gas_nick;
                    }
                    string name = v["name"][0]["name"].ToString();
                    return name;
                });
            return nameDict;
        }
       
        private JObject[] formatAssetName(JObject[] query, Dictionary<string, string> nameDict)
        {
            return query.Select(p =>
                   {
                       JObject jo = p;
                       string id = jo["asset"].ToString();
                       string idName = nameDict.GetValueOrDefault(id);
                       jo.Remove("asset");
                       jo.Add("asset", idName);
                       return jo;
                   }).ToArray();
        }

        private JArray formatAssetNameByIds(JArray query, string[] assetIds)
        {
            var nameDict = getAssetName(assetIds);
            return new JArray
                {
                    query.Select(p =>
                    {
                        JObject jo = (JObject)p;
                        string id = jo["asset"].ToString();
                        string idName = nameDict.GetValueOrDefault(id);
                        jo.Remove("asset");
                        jo.Add("asset", idName);
                        return jo;
                    }).ToArray()
                };
        }
        
    }
}
