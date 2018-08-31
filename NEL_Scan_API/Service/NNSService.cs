using NEL_Scan_API.lib;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace NEL_Scan_API.Service
{
    public class NNSService
    {
        public string newNotify_mongodbConnStr { set; get; }
        public string newNotify_mongodbDatabase { set; get; }
        public string bonusSgas_mongodbConnStr { set; get; }
        public string bonusSgas_mongodbDatabase { set; get; }
        public string bonusSgasCol { set; get; }
        public string nnsDomainState { get; set; }
        public string auctionStateColl { get; set; }
        public mongoHelper mh { set; get; }
        public string id_sgas { get; set; }
        private const long ONE_YEAR_SECONDS = 365 * 1 * /*24 * 60 * */60 /*测试时5分钟一天*/* 5;

        public JArray getStatistic()
        {
            // 奖金池 + 利息累计 + 已使用域名数量 + 正在竞拍域名数量
            int bonus = 0;
            decimal profit = getProfit();
            long usedDomainCount = mh.GetDataCount(newNotify_mongodbConnStr, newNotify_mongodbDatabase, nnsDomainState, toOrFilter("auctionState", new string[] { "0", "3" }).ToString());
            long auctingDomainCount = mh.GetDataCount(newNotify_mongodbConnStr, newNotify_mongodbDatabase, nnsDomainState, toOrFilter("auctionState", new string[] { "1", "2" }).ToString());
            return new JArray() { { new JObject() { { "bonus", bonus }, { "profit", profit }, { "usedDomainCount", usedDomainCount }, { "auctingDomainCount", auctingDomainCount } } } };
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
            string findStr = MongoFieldHelper.toFilter(new string[] {"0101", "0201", "0301" }, "auctionState").ToString();
            string sortStr = new JObject() { {"startTime.blockindex", -1 } }.ToString();
            string fieldStr = MongoFieldHelper.toReturn(new string[] {"fulldomain", "lastTime.txid", "maxBuyer", "maxPrice", "auctionState" }).ToString();
            JArray res = mh.GetDataPagesWithField(newNotify_mongodbConnStr, newNotify_mongodbDatabase, auctionStateColl, fieldStr, pageSize, pageNum, sortStr, findStr);
            if(res == null || res.Count == 0)
            {
                return new JArray() { };
            }
            long count = mh.GetDataCount(newNotify_mongodbConnStr, newNotify_mongodbDatabase, auctionStateColl, findStr);

            return new JArray() { { new JObject() { { "list", res }, { "count", count } } } };
        }
        public JArray getAuctingDomainList(int pageNum = 1, int pageSize = 10)
        {
            // 域名 + 哈希 + 当前最高价 + 竞标人 + 状态  ==> 哈希改为txid
            JObject filter = toOrFilter("auctionState", new string[] { "1", "2" });
            JObject sortBy = new JObject() { { "blockindex", -1 } };
            JObject fieldFilter = new JObject() { { "fulldomain", 1 }, { "txid", 1 }, { "maxBuyer", 1 }, { "maxPrice", 1 }, { "auctionState", 1 } };
            JArray res = mh.GetDataPagesWithField(newNotify_mongodbConnStr, newNotify_mongodbDatabase, nnsDomainState, fieldFilter.ToString(), pageSize, pageNum, sortBy.ToString(), filter.ToString());
            if(res == null || res.Count() == 0)
            {
                return new JArray() { };
            }
            long count = mh.GetDataCount(newNotify_mongodbConnStr, newNotify_mongodbDatabase, nnsDomainState, filter.ToString());

            return new JArray() { { new JObject() { { "list", res }, { "count", count } } } };
        }

        public JArray getUsedDomainListNew(int pageNum = 1, int pageSize = 10)
        {
            string findStr = MongoFieldHelper.toFilter(new string[] { "0401"}, "auctionState").ToString();
            string sortStr = new JObject() { { "maxPrice", -1 } }.ToString();
            string fieldStr = MongoFieldHelper.toReturn(new string[] { "fulldomain", "lastTime.txid", "maxBuyer", "maxPrice", "startTime.blocktime"}).ToString();
            JArray res = mh.GetDataPagesWithField(newNotify_mongodbConnStr, newNotify_mongodbDatabase, auctionStateColl, fieldStr, pageSize, pageNum, sortStr, findStr);
            if(res == null || res.Count() == 0)
            {
                return new JArray() { };
            }
            res = new JArray() { res.OrderByDescending(p => decimal.Parse(p["maxPrice"].ToString())).ToArray() };
            int num = (pageNum - 1) * pageSize;
            foreach (JObject obj in res)
            {
                obj.Add("range", ++num);
                obj.Add("ttl", long.Parse(obj["startTime"]["blocktime"].ToString()) + ONE_YEAR_SECONDS);
            }
            long count = mh.GetDataCount(newNotify_mongodbConnStr, newNotify_mongodbDatabase, auctionStateColl, findStr.ToString());
            return new JArray() { { new JObject() { { "list", res }, { "count", count } } } };
        }
        

        public JArray getUsedDomainList(int pageNum = 1, int pageSize = 10)
        {
            // 排名 + 域名 + 哈希 + 成交价 + 中标人 + 域名过期时间 ==> 哈希改为txid
            JObject filter = toOrFilter("auctionState", new string[] { "0", "3" });
            JObject sortBy = new JObject() { { "maxPrice", -1 } };
            JObject fieldFilter = new JObject() { { "fulldomain", 1 }, { "txid", 1 }, { "maxBuyer", 1 }, { "maxPrice", 1 }, { "ttl", 1 } };
            JArray res = mh.GetDataPagesWithField(newNotify_mongodbConnStr, newNotify_mongodbDatabase, nnsDomainState, fieldFilter.ToString(), pageSize, pageNum, sortBy.ToString(), filter.ToString());
            if (res == null || res.Count() == 0)
            {
                return new JArray() { };
            }
            long count = mh.GetDataCount(newNotify_mongodbConnStr, newNotify_mongodbDatabase, nnsDomainState, filter.ToString());

            int num = (pageNum - 1) * pageSize;
            foreach (JObject obj in res)
            {
                obj.Add("range", ++num);
            }
            return new JArray() { { new JObject() { { "list", res }, { "count", count } } } };
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

        public JArray getDomainInfo(string domain, bool domainInfoFlag = false)
        {
            // 1. 已经成交： 域名 + 哈希 + 成交价 + 中标人 + 域名过期时间
            // 2. 正在竞拍： 域名 + 哈希 + 当前最高价 + 中标人 + 状态
            // 3. 域名信息： 域名 + 哈希 + 竞拍开始时间 + 预计结束时间 + 当前最高价 + 竞标人 + 状态 + 区块高度
            // 4. 域名信息： 域名 + 哈希 + 竞拍开始时间 + 竞拍结束时间 + 成交价     + 中标人 + ttl  + 区块高度

            JArray res = mh.GetData(newNotify_mongodbConnStr, newNotify_mongodbDatabase, nnsDomainState, new JObject() { { "fulldomain", domain } }.ToString());
            if (res == null || res.Count() == 0)
            {
                return new JArray() { };
            }
            JObject obj = (JObject)res[0];
            string fulldomain = obj["fulldomain"].ToString();
            //string fulldomainhash = obj["fulldomainhash"].ToString();
            string maxPrice = obj["maxPrice"].ToString();
            string maxBuyer = obj["maxBuyer"].ToString();
            string auctionState = obj["auctionState"].ToString();
            string ttl = obj["ttl"].ToString();
            //string startAuctionTime = obj["startAuctionTime"].ToString();
            string startAuctionTime = obj["startBlockSellingTime"].ToString();
            string endBlockTime = obj["endBlockTime"].ToString();
            string blockindex = obj["blockindex"].ToString();
            string txid = obj["txid"].ToString();
            string id = obj["id"].ToString();
            if (auctionState == "0" || auctionState == "3" || auctionState == "5")
            {
                // 已经成交
                JObject Jdata = new JObject() { { "domain", fulldomain }, { "txid", txid }, { "maxPrice", maxPrice }, { "maxBuyer", maxBuyer }, { "ttl", ttl }, { "id", id } };
                //if(domainInfoFlag)
                {
                    Jdata.Add("startAuctionTime", startAuctionTime);
                    Jdata.Add("endBlockTime", endBlockTime);
                    Jdata.Add("blockindex", blockindex);
                }
                Jdata.Add("auctionState", "0");
                return new JArray() { Jdata };
            }
            if (auctionState == "1" || auctionState == "2")
            {
                // 正在竞拍
                JObject Jdata = new JObject() { { "domain", fulldomain }, { "txid", txid }, { "maxPrice", maxPrice }, { "maxBuyer", maxBuyer }, { "auctionState", auctionState }, { "id", id } };
                //if (domainInfoFlag)
                {
                    Jdata.Add("startAuctionTime", startAuctionTime);
                    Jdata.Add("endBlockTime", endBlockTime);
                    Jdata.Add("blockindex", blockindex);
                }
                return new JArray() { Jdata };
            }
            
            return new JArray() { };
        }

    }
}

