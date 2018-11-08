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

        public JArray gettransactionlist(int pageNum, int pageSize)
        {
            string findStr = new JObject() { { "blockindex", new JObject() { { "$gt", -1} } } }.ToString();
            long count = mh.GetDataCount(Block_mongodbConnStr, Block_mongodbDatabase, "tx", findStr);
            string fieldStr = MongoFieldHelper.toReturn(new string[] {"type", "txid", "blockindex", "size" }).ToString();
            string sortStr = new JObject() { { "blockindex", -1 } }.ToString();
            JArray query = mh.GetDataPagesWithField(Block_mongodbConnStr, Block_mongodbDatabase, "tx", fieldStr, pageSize, pageNum, sortStr, "{}");
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
                findStr = MongoFieldHelper.toFilter(assetIds.Distinct().ToArray(), "id").ToString();
                fieldStr = new JObject() { { "name.name", 1 }, {"id",1 } }.ToString();
                query = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "asset", fieldStr, findStr);
                var nameDict = 
                    query.ToDictionary(k => k["id"].ToString(), v =>
                    {
                        string id = v["id"].ToString();
                        if (id == AssetConst.id_neo)
                        {
                            return AssetConst.id_neo_nick;
                        }
                        if(id == AssetConst.id_gas)
                        {
                            return AssetConst.id_gas_nick;
                        }
                        string name = v["name"][0]["name"].ToString();
                        return name;
                    });
                if(vins != null)
                {
                    vins = vins.Select(p =>
                    {
                        JObject jo = (JObject)p;
                        string id = jo["asset"].ToString();
                        string idName = nameDict.GetValueOrDefault(id);
                        jo.Remove("asset");
                        jo.Add("asset", idName);
                        return jo;
                    }).ToArray();
                    tx.Remove("vin");
                    tx.Add("vin", new JArray { vins });
                }
                if (vouts != null)
                {
                    vouts = vouts.Select(p =>
                    {
                        JObject jo = (JObject)p;
                        string id = jo["asset"].ToString();
                        string idName = nameDict.GetValueOrDefault(id);
                        jo.Remove("asset");
                        jo.Add("asset", idName);
                        return jo;
                    }).ToArray();
                    tx.Remove("vout");
                    tx.Add("vout", new JArray { vouts });
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
    }
}
