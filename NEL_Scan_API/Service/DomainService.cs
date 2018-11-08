﻿using NEL_Scan_API.lib;
using NEL_Scan_API.Service.dao;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ThinNeo;

namespace NEL_Scan_API.Service
{
    public class DomainService
    {
        public mongoHelper mh { get; set; }
        public string Block_mongodbConnStr { get; set; }
        public string Block_mongodbDatabase { get; set; }
        public string Notify_mongodbConnStr { get; set; }
        public string Notify_mongodbDatabase { get; set; }
        public string auctionStateColl { get; set; }
        public string bonusAddress { get; set; }

        
        public JArray getDomainInfo(string fulldomain)
        {
            fulldomain = fulldomain.ToLower();
            string namehash = DomainHelper.nameHashFull(fulldomain);
            string findStr = new JObject() { {"namehash", namehash } }.ToString();
            string fieldStr = new JObject() { {"_id",0 },{"owner",1 }, { "TTL", 1 } }.ToString();
            string sortStr = new JObject() { {"blockindex",-1 } }.ToString();
            JArray res = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, "0xbd3fa97e2bc841292c1e77f9a97a1393d5208b48", fieldStr, 1,1, sortStr, findStr);
            return res;
        }

        private JArray format(JArray res)
        {
            if(res == null || res.Count == 0)
            {
                return new JArray() { };
            }
            return new JArray() {
                res.Select(p => {
                    JObject jo = (JObject)p;

                    // 格式转换
                    string value = jo["maxPrice"].ToString();
                    value = NumberDecimalHelper.formatDecimal(value);
                    jo.Remove("maxPrice");
                    jo.Add("maxPrice", value);

                    // 获取ttl
                    string fulldoamin = p["fulldomain"].ToString();
                    long ttl = 0;
                    var rr = getDomainInfo(fulldoamin);
                    if(rr != null && rr.Count > 0)
                    {
                        ttl = long.Parse(rr[0]["TTL"].ToString());
                        jo.Remove("ttl");
                        jo.Add("ttl", ttl);
                    }
                    
                    // 触发结束
                    long now = TimeHelper.GetTimeStamp();
                    long st = long.Parse(jo["startTime"]["blocktime"].ToString());
                    long ed = long.Parse(jo["endTime"]["blocktime"].ToString());
                    if(ed > 0)
                    {
                        jo.Remove("lastTime");
                        if(now >= ed && now <= ttl)
                        {
                            jo.Remove("auctionState");
                            jo.Add("auctionState", "0401");
                        }
                        return jo;
                    }

                    // 计算预计结束时间
                    TimeSetter timeSetter = TimeConst.getTimeSetter(fulldoamin.Substring(fulldoamin.LastIndexOf(".")));
                    string auctionState = p["auctionState"].ToString();
                    long expireSeconds = 0;
                    if (auctionState == "0201")
                    {
                        expireSeconds = st + timeSetter.THREE_DAY_SECONDS;
                    } else if(auctionState == "0301")
                    {
                        expireSeconds = st + timeSetter.FIVE_DAY_SECONDS;
                    } else if(auctionState == "0401")
                    {
                        long lt = long.Parse(jo["lastTime"]["blocktime"].ToString());
                        if(st + timeSetter.TWO_DAY_SECONDS >= lt)
                        {
                            expireSeconds = st + timeSetter.THREE_DAY_SECONDS;
                        } else
                        {
                            expireSeconds = st + timeSetter.FIVE_DAY_SECONDS;
                        }
                    } 
                    if(expireSeconds > 0)
                    {
                        JObject ep = (JObject)jo["endTime"];
                        long blocktime = long.Parse(ep["blocktime"].ToString());
                        long endBlockTime = blocktime + expireSeconds;
                        ep.Remove("blocktime");
                        jo.Remove("endTime");
                        ep.Add("blocktime", endBlockTime);
                        jo.Add("endTime", ep);
                        //
                        jo.Remove("lastTime");
                    }
                    if(now >= expireSeconds && now <= ttl && auctionState == "0601")
                    {
                        jo.Remove("auctionState");
                        jo.Add("auctionState", "0401");
                    }
                    
                    return jo;
                })
            };
        }
        public JArray searchByDomain(string fulldomain)
        {
            fulldomain = fulldomain.ToLower();
            // 域名信息:
            // 域名 + 哈希 + 开始时间 + 结束时间 + maxBuyer + maxPrice + auctionState + 开标块
            string findStr = new JObject() { { "fulldomain", fulldomain } }.ToString();
            string sortStr = new JObject() { { "startTime.blockindex", -1} }.ToString();
            string fieldStr = MongoFieldHelper.toReturn(new string[] { "fulldomain", "auctionId", "startTime.blocktime", "endTime.blocktime", "lastTime.blocktime", "maxBuyer", "maxPrice", "auctionState", "startTime.blockindex", "ttl" }).ToString();
            JArray res = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, auctionStateColl, fieldStr, 1, 1, sortStr, findStr);
            return format(res);
        }
        public JArray getAuctionInfo(string auctionId)
        {
            // 域名信息:
            // 域名 + 哈希 + 开始时间 + 结束时间 + maxBuyer + maxPrice + auctionState + 开标块
            string findStr = new JObject() { { "auctionId", auctionId } }.ToString();
            string fieldStr = MongoFieldHelper.toReturn(new string[] { "fulldomain", "auctionId", "startTime.blocktime", "endTime.blocktime", "lastTime.blocktime", "maxBuyer", "maxPrice", "auctionState", "startTime.blockindex", "ttl" }).ToString() ;
            JArray res = mh.GetDataWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, auctionStateColl, fieldStr, findStr);
            return format(res);

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
            
            //JToken[] arr = jt.Where(p => p["address"].ToString() != bonusAddress).OrderByDescending(p => decimal.Parse(p["totalValue"].ToString(), NumberStyles.Float)).ToArray();
            JToken[] arr = jt.Select(p =>
            {
                JObject jo = (JObject)p;
                string value = jo["totalValue"].ToString();
                value = NumberDecimalHelper.formatDecimal(value);
                jo.Remove("totalValue");
                jo.Add("totalValue", value);
                return jo;
            }).Where(p => p["address"].ToString() != bonusAddress).OrderByDescending(p => decimal.Parse(p["totalValue"].ToString(), NumberStyles.Float)).ToArray();
            long count = arr.Count();
            int num = (pageNum - 1) * pageSize;
            JArray js = new JArray();
            foreach (JObject obj in arr.Skip(num).Take(pageSize))
            {
                obj.Add("range", ++num);
                js.Add(obj);
            }
            return new JArray() { { new JObject() { { "list", js }, { "count", count } } } };
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
                { "type", AuctionStatus.AuctionStatus_Start},
                { "address", tx.startAddress},
                { "amount", 0 },
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
                    jo.Add("type", AuctionStatus.AuctionStatus_AddPrice);
                    jo.Add("address", addwho.address);
                    jo.Add("amount", addprice.value);
                    jo.Add("time", addprice.time.blocktime);
                    arr.Add(jo);
                }
                if(addwho.accountTime != null && addwho.accountTime.blockindex != 0)
                {
                    jo = new JObject();
                    jo.Add("txid", addwho.accountTime.txid);
                    jo.Add("type", AuctionStatus.AuctionStatus_Account);
                    jo.Add("address", addwho.address);
                    jo.Add("amount", getEndValue(addwho.addpricelist));
                    jo.Add("time", addwho.accountTime.blocktime);
                    arr.Add(jo);
                }
                if (addwho.getdomainTime != null && addwho.getdomainTime.blockindex != 0)
                {
                    jo = new JObject();
                    jo.Add("txid", addwho.getdomainTime.txid);
                    jo.Add("type", AuctionStatus.AuctionStatus_GetDomain);
                    jo.Add("address", addwho.address);
                    jo.Add("amount", getEndValue(addwho.addpricelist));
                    jo.Add("time", addwho.getdomainTime.blocktime);
                    arr.Add(jo);
                }

            }
            if(tx.endTime != null && tx.endTime.blockindex !=0)
            {
                jo = new JObject() {
                { "txid", tx.endTime.txid},
                { "type", AuctionStatus.AuctionStatus_End},
                { "address", tx.endAddress},
                { "amount", 0 },
                { "time", tx.endTime.blocktime } };
                arr.Add(jo);
            }
            JToken[] jt = arr.OrderByDescending(p => long.Parse(p["time"].ToString())).ToArray();
            int count = jt.Count();
            JArray res = new JArray() { jt.Skip(pageSize * (pageNum-1)).Take(pageSize).ToArray() };
            return new JArray() { new JObject() { { "count", count }, { "list", res } } };
        }

        private decimal getEndValue(List<AuctionAddPrice> list)
        {
            if (list == null || list.Count() == 0)
            {
                return 0;
            }
            AuctionAddPrice[] ls = list.Where(p => p.isEnd == "1").ToArray();
            if (ls == null || ls.Count() == 0)
            {
                return 0;
            }
            return Math.Abs(ls[0].value);

        }

        static class AuctionStatus
        {
            public static string AuctionStatus_Start = "4001";            // 开标
            public static string AuctionStatus_AddPrice = "4002";         // 加价
            public static string AuctionStatus_End = "4003";              // 结束
            public static string AuctionStatus_Account = "4004";          // 取回Gas
            public static string AuctionStatus_GetDomain = "4005";        // 领取域名
        }
        
        private Dictionary<string, long> getBlockTime(long[] blockindexArr)
        {
            JObject queryFilter = MongoFieldHelper.toFilter(blockindexArr, "index", "$or");
            JObject returnFilter = MongoFieldHelper.toReturn(new string[] { "index", "time" });
            JArray blocktimeRes = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "block", returnFilter.ToString(), queryFilter.ToString());
            return blocktimeRes.ToDictionary(key => key["index"].ToString(), val => long.Parse(val["time"].ToString()));
        }

        private string getNameHash(string domain)
        {
            return "0x" + Helper.Bytes2HexString(new NNSUrl(domain).namehash.Reverse().ToArray());
        }
    }
}