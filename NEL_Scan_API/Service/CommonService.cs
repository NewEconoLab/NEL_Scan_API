using NEL_Scan_API.lib;
using NEL_Scan_API.Service.dao;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using ThinNeo;

namespace NEL_Scan_API.Service
{
    public class CommonService
    {
        public mongoHelper mh { get; set; }
        public string Block_mongodbConnStr { get; set; }
        public string Block_mongodbDatabase { get; set; }
        public string Notify_mongodbConnStr { get; set; }
        public string Notify_mongodbDatabase { get; set; }
        public string queryBidListCollection { get; set; }
        public string auctionStateColl { get; set; }
        private const long ONE_YEAR_SECONDS = 365 * 1 * /*24 * 60 * */60 /*测试时5分钟一天*/* 5;
        public string bonusAddress { get; set; }

        public JArray searchByDomain(string fulldomain)
        {
            // 域名信息:
            // 域名 + 哈希 + 开始时间 + 结束时间 + maxBuyer + maxPrice + auctionState + 开标块
            string findStr = new JObject() { { "fulldomain", fulldomain } }.ToString();
            string sortStr = new JObject() { { "startTime.blockindex", -1} }.ToString();
            string fieldStr = MongoFieldHelper.toReturn(new string[] { "fulldomain", "auctionId", "startTime.blocktime", "endTime.blocktime", "maxBuyer", "maxPrice", "auctionState", "startTime.blockindex", "ttl" }).ToString();

            return mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, auctionStateColl, fieldStr, 1, 1, sortStr, findStr);
        }
        public JArray getAuctionInfo(string auctionId)
        {
            // 域名信息:
            // 域名 + 哈希 + 开始时间 + 结束时间 + maxBuyer + maxPrice + auctionState + 开标块
            string findStr = new JObject() { { "auctionId", auctionId } }.ToString();
            string fieldStr = MongoFieldHelper.toReturn(new string[] { "fulldomain", "auctionId", "startTime.blocktime", "endTime.blocktime", "maxBuyer", "maxPrice", "auctionState", "startTime.blockindex", "ttl" }).ToString() ;
            return  mh.GetDataWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, auctionStateColl, fieldStr, findStr);
            
        }
        public JArray getAuctionInfoRank(string auctionId, int pageNum=1, int pageSize=10)
        {
            // 竞价排行:
            // 排名 + 价格 + 竞标人
            string findStr = new JObject() { { "auctionId", auctionId } }.ToString();
            string fieldStr = MongoFieldHelper.toReturn(new string[] { "addwholist.address", "addwholist.totalValue" }).ToString();
            JArray res = mh.GetDataWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, auctionStateColl, fieldStr, findStr);
            if(res == null || res.Count == 0)
            {
                return new JArray() { };
            }
            JToken[] jt = res.SelectMany(p =>
            {
                JObject ja = (JObject)p;
                return (JArray)ja["addwholist"];
            }).ToArray();


            JToken[] arr = jt.Where(p => p["address"].ToString() != bonusAddress).OrderByDescending(p => decimal.Parse(p["totalValue"].ToString())).ToArray();
            long count = arr.Count();
            int num = (pageNum - 1) * pageSize; ;
            foreach (JObject obj in arr.Skip(num))
            {
                obj.Add("range", ++num);
            }
            res = new JArray() { arr };
            return new JArray() { { new JObject() { { "list", res }, { "count", count } } } };
        }
        public JArray getAuctionInfoTx(string auctionId, int pageNum=1, int pageSize=10)
        {
            // 竞拍信息:
            // txid + 类型(开标+加价+结束+领取+取回+) + 地址 + 金额 + 时间
            string findStr = new JObject() { { "auctionId", auctionId } }.ToString();
            List<AuctionTx> txList = mh.GetData<AuctionTx>(Notify_mongodbConnStr, Notify_mongodbDatabase, auctionStateColl, findStr);
            if(txList == null || txList.Count == 0)
            {
                return new JArray() { };
            }
            AuctionTx tx = txList[0];

            //txid + 类型(开标+加价+结束+领取+取回+) + 地址 + 金额 + 时间
            JArray arr = new JArray();
            JObject jo = new JObject() {
                { "txid", tx.startTime.txid},
                { "type", AuctionStaus.AuctionStatus_Start},
                { "address", tx.startAddress},
                { "amount", "0" },
                { "time", tx.startTime.blocktime } };
            arr.Add(jo);
            foreach (AuctionAddWho addwho in tx.addwholist)
            {
                if(bonusAddress == addwho.address) continue; 

                foreach(AuctionAddPrice addprice in addwho.addpricelist)
                {
                    if (addprice.value <= 0) continue;
                    jo = new JObject();
                    jo.Add("txid", addprice.time.txid);
                    jo.Add("type", AuctionStaus.AuctionStatus_AddPrice);
                    jo.Add("address", addwho.address);
                    jo.Add("amount", addprice.value);
                    jo.Add("time", addprice.time.blocktime);
                    arr.Add(jo);
                }
                if(addwho.accountTime != null && addwho.accountTime.blockindex != 0)
                {
                    jo = new JObject();
                    jo.Add("txid", addwho.accountTime.txid);
                    jo.Add("type", AuctionStaus.AuctionStatus_Account);
                    jo.Add("address", addwho.address);
                    jo.Add("amount", "0");
                    jo.Add("time", addwho.accountTime.blocktime);
                    arr.Add(jo);
                }
                if (addwho.getdomainTime != null && addwho.getdomainTime.blockindex != 0)
                {
                    jo = new JObject();
                    jo.Add("txid", addwho.getdomainTime.txid);
                    jo.Add("type", AuctionStaus.AuctionStatus_GetDomain);
                    jo.Add("address", addwho.address);
                    jo.Add("amount", "0");
                    jo.Add("time", addwho.getdomainTime.blocktime);
                    arr.Add(jo);
                }

            }
            if(tx.endTime != null && tx.endTime.blockindex !=0)
            {
                jo = new JObject() {
                { "txid", tx.endTime.txid},
                { "type", AuctionStaus.AuctionStatus_End},
                { "address", tx.endAddress},
                { "amount", "0" },
                { "time", tx.endTime.blocktime } };
                arr.Add(jo);
            }
            JToken[] jt = arr.OrderByDescending(p => long.Parse(p["time"].ToString())).ToArray();
            int count = jt.Count();
            JArray res = new JArray() { jt.Skip(pageSize * (pageNum-1)).Take(pageSize).ToArray() };
            return new JArray() { new JObject() { { "count", count }, { "list", res } } };
        }

        static class AuctionStaus
        {
            public static string AuctionStatus_Start = "500301";            // 开标
            public static string AuctionStatus_AddPrice = "500302";         // 加价
            public static string AuctionStatus_End = "500303";              // 结束
            public static string AuctionStatus_Account = "500304";          // 取回Gas
            public static string AuctionStatus_GetDomain = "500305";        // 领取域名
        }



        public JArray getBidDetailByAuctionId(string auctionId, int pageNum = 1, int pageSize = 10)
        {
            if (!auctionId.StartsWith("0x"))
            {
                auctionId = "0x" + auctionId;
            }
            JObject filter = new JObject();
            filter.Add("id", auctionId);
            filter.Add("displayName", "addprice");
            filter.Add("maxPrice", new JObject() { { "$ne", "0" } });
            // 累加value需要查询所有记录
            JArray queryRes = mh.GetData(Notify_mongodbConnStr, Notify_mongodbDatabase, queryBidListCollection, filter.ToString());
            if (queryRes == null || queryRes.Count == 0)
            {
                return new JArray() { };
            }
            // 批量查询blockindex对应的时间
            long[] blockindexArr = queryRes.Select(item => long.Parse(item["blockindex"].ToString())).ToArray();
            Dictionary<string, long> blocktimeDict = getBlockTime(blockindexArr);
            // 
            JObject[] arr = queryRes.Select(item =>
            {
                string maxPrice = item["maxPrice"].ToString();
                string maxBuyer = item["maxBuyer"].ToString();
                string who = item["who"].ToString();
                if (maxBuyer != who)
                {
                    maxBuyer = who;
                    maxPrice = Convert.ToString(queryRes.Where(pItem => pItem["who"].ToString() == who && int.Parse(pItem["blockindex"].ToString()) <= int.Parse(item["blockindex"].ToString())).Sum(ppItem => double.Parse(ppItem["value"].ToString())));
                }
                long addPriceTime = blocktimeDict.GetValueOrDefault(Convert.ToString(item["blockindex"]));
                // 新增txid +出价人 +当笔出价金额
                string txid = item["txid"].ToString();
                string bidder = item["who"].ToString();
                double raisebid = double.Parse(item["value"].ToString());
                return new JObject() { { "maxPrice", maxPrice }, { "maxBuyer", maxBuyer }, { "addPriceTime", addPriceTime }, { "txid", txid }, { "bidder", bidder }, { "raisebid", raisebid } };

            }).OrderByDescending(p => double.Parse(p["maxPrice"].ToString())).ThenByDescending(p => p["addPriceTime"]).ToArray();
            // 返回
            JObject res = new JObject();
            res.Add("count", arr.Count());
            res.Add("list", new JArray() { arr.Skip(pageSize * (pageNum - 1)).Take(pageSize).ToArray() });
            return new JArray() { res };
        }
        public JArray getBidDetailByDomain(string domain, int pageNum = 1, int pageSize = 10)
        {
            string[] domainArr = domain.Split(".");
            JObject filter = new JObject();
            filter.Add("domain", domainArr[0]);
            filter.Add("parenthash", getNameHash(domainArr[1]));
            filter.Add("displayName", "addprice");
            // 累加value需要查询所有记录
            JArray queryRes = mh.GetData(Notify_mongodbConnStr, Notify_mongodbDatabase, queryBidListCollection, filter.ToString());
            if (queryRes == null || queryRes.Count == 0)
            {
                return new JArray() { };
            }
            // 批量查询blockindex对应的时间
            long[] blockindexArr = queryRes.Select(item => long.Parse(item["blockindex"].ToString())).ToArray();
            Dictionary<string, long> blocktimeDict = getBlockTime(blockindexArr);
            // 最近一次开拍时间开始之后的竞拍记录
            long lastStartBlockSelling = queryRes.Where(p => p["maxPrice"].ToString() == "0").Select(p => long.Parse(p["blockindex"].ToString())).OrderByDescending(p => p).ToArray()[0];
            JToken[] queryArr = queryRes.Where(p => long.Parse(p["blockindex"].ToString()) >= lastStartBlockSelling).ToArray();
            // 分页
            JObject[] arr = queryArr.Select(item =>
            {
                string maxPrice = item["maxPrice"].ToString();
                string maxBuyer = item["maxBuyer"].ToString();
                string who = item["who"].ToString();
                if (maxBuyer != who)
                {
                    maxBuyer = who;
                    maxPrice = Convert.ToString(queryArr.Where(pItem => pItem["who"].ToString() == who && int.Parse(pItem["blockindex"].ToString()) <= int.Parse(item["blockindex"].ToString())).Sum(ppItem => double.Parse(ppItem["value"].ToString())));
                }
                long addPriceTime = blocktimeDict.GetValueOrDefault(Convert.ToString(item["blockindex"]));
                // 新增txid +出价人 +当笔出价金额
                string txid = item["txid"].ToString();
                string bidder = item["who"].ToString();
                double raisebid = double.Parse(item["value"].ToString());
                return new JObject() { { "maxPrice", maxPrice }, { "maxBuyer", maxBuyer }, { "addPriceTime", addPriceTime }, { "txid", txid }, { "bidder", bidder }, { "raisebid", raisebid } };

            }).Where(p => Convert.ToString(p["maxPrice"]) != "0").OrderByDescending(p => p["addPriceTime"]).ThenByDescending(p => double.Parse(p["maxPrice"].ToString())).Skip(pageSize * (pageNum - 1)).Take(pageSize).ToArray();
            // 总量
            long count = queryArr.Where(p => p["maxPrice"].ToString() != "0").ToArray().Count();
            // 返回
            JObject res = new JObject();
            res.Add("list", new JArray() { arr });
            res.Add("count", count);
            return new JArray() { res };
        }

        private Dictionary<string, long> getBlockTime(long[] blockindexArr)
        {
            JObject queryFilter = toFilter(blockindexArr, "index", "$or");
            JObject returnFilter = toReturn(new string[] { "index", "time" });
            JArray blocktimeRes = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "block", returnFilter.ToString(), queryFilter.ToString());
            return blocktimeRes.ToDictionary(key => key["index"].ToString(), val => long.Parse(val["time"].ToString()));
        }

        private JObject toFilter(long[] blockindexArr, string field, string logicalOperator = "$or")
        {
            if (blockindexArr.Count() == 1)
            {
                return new JObject() { { field, blockindexArr[0] } };
            }
            return new JObject() { { logicalOperator, new JArray() { blockindexArr.Select(item => new JObject() { { field, item } }).ToArray() } } };
        }
        private JObject toReturn(string[] fieldArr)
        {
            JObject obj = new JObject();
            foreach (var field in fieldArr)
            {
                obj.Add(field, 1);
            }
            return obj;
        }
        private JObject toSort(string[] fieldArr, bool order = false)
        {
            int flag = order ? 1 : -1;
            JObject obj = new JObject();
            foreach (var field in fieldArr)
            {
                obj.Add(field, flag);
            }
            return obj;
        }
        private string getNameHash(string domain)
        {
            return "0x" + Helper.Bytes2HexString(new NNSUrl(domain).namehash.Reverse().ToArray());
        }
    }
}
