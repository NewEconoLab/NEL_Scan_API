using NEL_Scan_API.lib;
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
