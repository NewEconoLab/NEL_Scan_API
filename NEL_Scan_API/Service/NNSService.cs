using NEL_Scan_API.lib;
using NEL_Scan_API.Service.dao;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NEL_Scan_API.Service
{
    public class NNSService
    {
        public mongoHelper mh { set; get; }
        public string block_mongodbConnStr { set; get; }
        public string block_mongodbDatabase { set; get; }
        public string analy_mongodbConnStr { set; get; }
        public string analy_mongodbDatabase { set; get; }
        public string notify_mongodbConnStr { set; get; }
        public string notify_mongodbDatabase { set; get; }
        public string bonusSgas_mongodbConnStr { set; get; }
        public string bonusSgas_mongodbDatabase { set; get; }
        public string bonusStatisticCol { set; get; }
        public string bonusSgasCol { set; get; }
        public string auctionStateColl { get; set; }
        
        public string id_sgas { get; set; }
        public string bonusAddress { get; set; }
        public string nelJsonRPCUrl { get; set; }


        public JArray getNNSFixedSellingList(string orderField = "price"/* time/price */, string orderType = "high"/* hight/low */, int pageNum = 1, int pageSize = 10)
        {
            string findStr = new JObject() { { "displayName", "NNSfixedSellingLaunched" } }.ToString();
            long count = mh.GetDataCount(notify_mongodbConnStr, notify_mongodbDatabase, "nnsFixedSellingState", findStr);
            if (count == 0) return new JArray { };


            // domain + price + launchtime + owner + ttl
            string fieldStr = MongoFieldHelper.toReturn(new string[] {"fullDomain", "price", "launchTime", "owner", "ttl" }).ToString();
            string sortStr = new JObject() { { "blockindex", -1 } }.ToString();
            if (orderField == "time")
            {
                // default
            } else if(orderField == "price")
            {
                sortStr = new JObject() { { "price", orderType == "high" ? -1:1 } }.ToString();
            } 

            var query = mh.GetDataPagesWithField(notify_mongodbConnStr, notify_mongodbDatabase, "nnsFixedSellingState", fieldStr, pageSize, pageNum, sortStr, findStr);
            var res = query.Select(p =>
            {
                JObject jo = (JObject)p;
                string price = NumberDecimalHelper.formatDecimal(jo["price"].ToString());
                jo.Remove("price");
                jo.Add("price", price);
                return jo;
            }).ToArray();
            /*
            // 获取owner + ttl
            Dictionary<string,string> fullhashDict = query.ToDictionary(k => DomainHelper.nameHashFull(k["fullDomain"].ToString()), v => v["fullDomain"].ToString());
            findStr = MongoFieldHelper.toFilter(fullhashDict.Keys.ToArray(), "namehash").ToString();
            fieldStr = MongoFieldHelper.toReturn(new string[]{"namehash","owner", "TTL" }).ToString();
            var subquery = mh.GetDataWithField(notify_mongodbConnStr, notify_mongodbDatabase, "domainOwnerCol", fieldStr, findStr);
            Dictionary<string, JObject> ownerDict = subquery.ToDictionary(k => fullhashDict.GetValueOrDefault(k["namehash"].ToString()), v => (JObject)v);
            
            // 获取blocktime
            long[] blockindexArr = query.Select(p => long.Parse(p["blockindex"].ToString())).Distinct().ToArray();
            findStr = MongoFieldHelper.toFilter(blockindexArr, "index").ToString();
            fieldStr = MongoFieldHelper.toReturn(new string[] { "index", "time" }).ToString();
            subquery = mh.GetDataWithField(block_mongodbConnStr, block_mongodbDatabase, "block", fieldStr, findStr);
            Dictionary<string, long> blocktimeDict = subquery.ToDictionary(k => k["index"].ToString(), v=>long.Parse(v["time"].ToString()));

            var res = 
            query.Select(p =>
            {

                JObject jo = (JObject)p;
                string fullDomain = jo["fullDomain"].ToString();
                jo.Add("owner", ownerDict.GetValueOrDefault(fullDomain)["owner"]);
                jo.Add("TTL", ownerDict.GetValueOrDefault(fullDomain)["TTL"]);
                jo.Add("time", blocktimeDict.GetValueOrDefault(jo["blockindex"].ToString()));
                jo.Remove("blockindex");
                return jo;
            }).ToArray();

            */
            return new JArray() { { new JObject() {
                {"count", count },
                { "list", new JArray{ res } }
            } } };
        }
        public JArray getStatistic()
        {
            // 奖金池 + 利息累计 + 已使用域名数量 + 正在竞拍域名数量
            decimal bonus = getBonus();
            decimal profit = getProfit();
            long auctingDomainCount = mh.GetDataCount(notify_mongodbConnStr, notify_mongodbDatabase, auctionStateColl, toOrFilter("auctionState", new string[] { AuctionState.STATE_CONFIRM, AuctionState.STATE_RANDOM }).ToString());
            long usedDomainCount = mh.GetDataCount(notify_mongodbConnStr, notify_mongodbDatabase, auctionStateColl, toOrFilter("auctionState", new string[] { AuctionState.STATE_END }).ToString());
            return new JArray() { { new JObject() { { "bonus", bonus }, { "profit", profit }, { "usedDomainCount", usedDomainCount }, { "auctingDomainCount", auctingDomainCount } } } };
        }
        private decimal getBonus()
        {
            string findStr = new JObject() { { "addr", bonusAddress }, {"asset", id_sgas } }.ToString();
            JArray res = mh.GetData(analy_mongodbConnStr, analy_mongodbDatabase, bonusStatisticCol, findStr);
            if(res == null || res.Count == 0)
            {
                return 0;
            }
            return 
            decimal.Parse(res[0]["value_pre"].ToString(), NumberStyles.Float) +
            decimal.Parse(res[0]["value_cur"].ToString(), NumberStyles.Float);
            /*
            string addressHash = Helper.Bytes2HexString(Helper.GetPublicKeyHashFromAddress(bonusAddress));
            var result = TxHelper.api_InvokeScript(nelJsonRPCUrl, new ThinNeo.Hash160(id_sgas), "balanceOf", "(bytes)" + addressHash);
            var bonusRes = result.Result.value.subItem[0].AsInteger();
            return decimal.Parse(bonusRes.ToString().getNumStrFromIntStr(8), NumberStyles.Float);
            */
        }
        private decimal getProfit()
        {
            string filter = new JObject() { {"assetid",id_sgas } }.ToString();
            JArray rr = mh.GetData(bonusSgas_mongodbConnStr, bonusSgas_mongodbDatabase, bonusSgasCol, filter);
            if(rr != null && rr.Count > 0)
            {
                return rr.Select(p => decimal.Parse(NumberDecimalHelper.formatDecimal(p["totalValue"].ToString()), NumberStyles.Float)).Sum();
            }
            return 0;
        }
        
        public JArray getAuctingDomainList(int pageNum = 1, int pageSize = 10, bool sortByMaxPrice=false)
        {
            string findStr = MongoFieldHelper.toFilter(new string[] { AuctionState.STATE_CONFIRM, AuctionState.STATE_RANDOM }, "auctionState").ToString();
            string sortStr = null;
            if (sortByMaxPrice)
            {
                sortStr = new JObject() { { "maxPrice", -1 } }.ToString();
            } else
            {
                sortStr = new JObject() { { "startTime.blockindex", -1 } }.ToString();
            }
            string fieldStr = MongoFieldHelper.toReturn(new string[] { "fulldomain", "lastTime.txid", "maxBuyer", "maxPrice", "auctionState" }).ToString();
            JArray res = mh.GetDataPagesWithField(notify_mongodbConnStr, notify_mongodbDatabase, auctionStateColl, fieldStr, pageSize, pageNum, sortStr, findStr);
            if (res == null || res.Count == 0)
            {
                return new JArray() { };
            }
            res = new JArray() {
                res.Select(p =>
                {
                    JObject jo = (JObject)p;
                    string value = jo["maxPrice"].ToString();
                    value = NumberDecimalHelper.formatDecimal(value);
                    jo.Remove("maxPrice");
                    jo.Add("maxPrice", value);
                    return jo;
                }).ToArray()
            };
            long count = mh.GetDataCount(notify_mongodbConnStr, notify_mongodbDatabase, auctionStateColl, findStr);
            return new JArray() { { new JObject() { { "list", res }, { "count", count } } } };
        }
        public JArray getAuctingDomainListByMaxPrice(int pageNum = 1, int pageSize = 10)
        {
            return getAuctingDomainList(pageNum, pageSize, true);
        }


        public JArray getUsedDomainList(int pageNum = 1, int pageSize = 10)
        {
            string findStr = MongoFieldHelper.toFilter(new string[] { AuctionState.STATE_END }, "auctionState").ToString();
            string sortStr = new JObject() { { "maxPrice", -1 } }.ToString();
            string fieldStr = MongoFieldHelper.toReturn(new string[] { "fulldomain", "lastTime.txid", "maxBuyer", "maxPrice", "startTime.blocktime", "ttl" }).ToString();
            JArray res = mh.GetDataPagesWithField(notify_mongodbConnStr, notify_mongodbDatabase, auctionStateColl, fieldStr, pageSize, pageNum, sortStr, findStr);
            if (res == null || res.Count() == 0)
            {
                return new JArray() { };
            }
            JArray ja = new JArray();
            int num = (pageNum-1)*pageSize;
            foreach (JObject obj in res)
            {
                obj.Add("range", ++num);
                string value = obj["maxPrice"].ToString();
                value = NumberDecimalHelper.formatDecimal(value);
                obj.Remove("maxPrice");
                obj.Add("maxPrice", value);
                ja.Add(obj);
            }
            long count = mh.GetDataCount(notify_mongodbConnStr, notify_mongodbDatabase, auctionStateColl, findStr);
            return new JArray() { { new JObject() { { "list", format(ja) }, { "count", count } } } };
        }

        private JArray format(JArray res)
        {
            TimeSetter timeSetter = TimeConst.getTimeSetter(".test");
            return new JArray()
            {
                res.Select(p => {
                    string fulldomain = p["fulldomain"].ToString();
                    if(fulldomain.EndsWith(".test"))
                    {
                        JObject jo = (JObject)p;
                        long starttime = long.Parse(jo["startTime"]["blocktime"].ToString());
                        jo.Remove("ttl");
                        jo.Add("ttl", starttime + timeSetter.ONE_YEAR_SECONDS);
                        return jo;
                    }
                    return p;
                })
            };
        }

        private JObject toOrFilter(string field, string[] filter)
        {
            if (filter == null || filter.Count() == 0)
            {
                return null;
            }
            if (filter.Count() == 1)
            {
                return new JObject() { { field, filter[0] } };
            }
            return new JObject() { { "$or", new JArray() { filter.Select(p => new JObject() { { field, p } }) } } };
        }

    }
}

