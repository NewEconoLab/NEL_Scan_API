using NEL_Scan_API.lib;
using NEL_Scan_API.Service.dao;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using ThinNeo;

namespace NEL_Scan_API.Service
{
    public class NNSService
    {
        public string notify_mongodbConnStr { set; get; }
        public string notify_mongodbDatabase { set; get; }
        public string bonusSgas_mongodbConnStr { set; get; }
        public string bonusSgas_mongodbDatabase { set; get; }
        public string bonusSgasCol { set; get; }
        public string auctionStateColl { get; set; }
        public mongoHelper mh { set; get; }
        public string id_sgas { get; set; }
        public string bonusAddress { get; set; }
        public string nelJsonRPCUrl { get; set; }

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
            string addressHash = Helper.Bytes2HexString(Helper.GetPublicKeyHashFromAddress(bonusAddress));
            var result = TxHelper.api_InvokeScript(nelJsonRPCUrl, new ThinNeo.Hash160(id_sgas), "balanceOf", "(bytes)" + addressHash);
            var bonusRes = result.Result.value.subItem[0].AsInteger();
            return decimal.Parse(bonusRes.ToString().getNumStrFromIntStr(8));
        }
        private decimal getProfit()
        {
            string filter = new JObject() { {"assetid",id_sgas } }.ToString();
            JArray rr = mh.GetData(bonusSgas_mongodbConnStr, bonusSgas_mongodbDatabase, bonusSgasCol, filter);
            if(rr != null && rr.Count > 0)
            {
                return rr.Select(p => Decimal.Parse(p["totalValue"].ToString())).Sum();
            }
            return 0;
        }

        public JArray getAuctingDomainListNew(int pageNum = 1, int pageSize = 10)
        {
            string findStr = MongoFieldHelper.toFilter(new string[] { AuctionState.STATE_CONFIRM, AuctionState.STATE_RANDOM }, "auctionState").ToString();
            string sortStr = new JObject() { {"startTime.blockindex", -1 } }.ToString();
            string fieldStr = MongoFieldHelper.toReturn(new string[] {"fulldomain", "lastTime.txid", "maxBuyer", "maxPrice", "auctionState" }).ToString();
            JArray res = mh.GetDataPagesWithField(notify_mongodbConnStr, notify_mongodbDatabase, auctionStateColl, fieldStr, pageSize, pageNum, sortStr, findStr);
            if(res == null || res.Count == 0)
            {
                return new JArray() { };
            }
            long count = mh.GetDataCount(notify_mongodbConnStr, notify_mongodbDatabase, auctionStateColl, findStr);

            return new JArray() { { new JObject() { { "list", res }, { "count", count } } } };
        }
        public JArray getUsedDomainListNew(int pageNum = 1, int pageSize = 10)
        {
            string findStr = MongoFieldHelper.toFilter(new string[] { AuctionState.STATE_END}, "auctionState").ToString();
            string fieldStr = MongoFieldHelper.toReturn(new string[] { "fulldomain", "lastTime.txid", "maxBuyer", "maxPrice", "startTime.blocktime","ttl"}).ToString();
            JArray res = mh.GetDataWithField(notify_mongodbConnStr, notify_mongodbDatabase, auctionStateColl, fieldStr, findStr);
            if(res == null || res.Count() == 0)
            {
                return new JArray() { };
            }
            res = new JArray() { res.OrderByDescending(p => decimal.Parse(p["maxPrice"].ToString())).ToArray() };
            int num = (pageNum - 1) * pageSize;
            JArray ja = new JArray();
            foreach (JObject obj in res.Skip(num).Take(pageSize))
            {
                obj.Add("range", ++num);
                ja.Add(obj);
            }
            long count = res.Count();
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
                        jo.Add("ttl", starttime + timeSetter.ONE_DAY_SECONDS);
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

