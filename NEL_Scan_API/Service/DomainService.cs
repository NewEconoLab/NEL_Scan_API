using NEL_Scan_API.lib;
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
        public string domainCenterColl { get; set; } = "0xbd3fa97e2bc841292c1e77f9a97a1393d5208b48";
        public string NNSfixedSellingColl { get; set; } = "0x7a64879a21b80e96a8bc91e0f07adc49b8f3521e";
        public string NNsfixedSellingAddr { get; set; }
        
        public JArray getDomainTransferAndSellingInfoNew(string domain, int pageNum=1, int pageSize=10)
        {
            domain = domain.ToLower();
            string namehash = DomainHelper.nameHashFull(domain);
            string findStr = new JObject() { { "fullHash", namehash },{"displayName", "NNSfixedSellingBuy" } }.ToString();
            var count = mh.GetDataCount(Notify_mongodbConnStr, Notify_mongodbDatabase, NNSfixedSellingColl, findStr);
            if (count == 0) return new JArray { };
            
            string fieldStr = new JObject() { { "seller", 1 }, { "blockindex", 1 }, { "price", 1 } }.ToString();
            string sortStr = new JObject() { { "blockindex", -1 } }.ToString();
            var query = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, NNSfixedSellingColl, fieldStr, pageSize, pageNum, sortStr, findStr);

            JObject[] res = new JObject[0];
            if (query != null && query.Count > 0)
            {
                long[] indexs = query.Select(p => long.Parse(p["blockindex"].ToString())).Distinct().ToArray();
                findStr = MongoFieldHelper.toFilter(indexs, "index").ToString();
                fieldStr = new JObject() { { "time", 1 }, { "index", 1 } }.ToString();
                var timeRes = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "block", fieldStr, findStr);
                Dictionary<string, long> timeDict = null;
                if (timeRes != null && timeRes.Count > 0)
                {
                    timeDict = timeRes.ToDictionary(k => k["index"].ToString(), v => long.Parse(v["time"].ToString()));
                }
                res = query.Select(p =>
                {
                    JObject jo = (JObject)p;
                    long time = timeDict.GetValueOrDefault(jo["blockindex"].ToString());
                    jo.Add("time", time);
                    jo.Remove("blockindex");
                    return jo;
                }).ToArray();
            }
            return new JArray() { new JObject() { { "count", count }, { "list", new JArray { res } } } };
        }
        public JArray getDomainTransferAndSellingInfo(string domain, int pageNum=1, int pageSize=10)
        {
            bool flag = true;
            if(flag)
            {
                return getDomainTransferAndSellingInfoNew(domain, pageNum, pageSize);
            }
            domain = domain.ToLower();
            string namehash = DomainHelper.nameHashFull(domain);
            string findStr = new JObject() { { "namehash", namehash },{"owner", new JObject() { {"$ne", NNsfixedSellingAddr } } } }.ToString();
            string fieldStr = new JObject() { {"owner",1 },{ "blockindex", 1},{ "txid", 1} }.ToString();
            string sortStr = new JObject() { { "blockindex", -1} }.ToString();
            var count = mh.GetDataCount(Notify_mongodbConnStr, Notify_mongodbDatabase, domainCenterColl, findStr);
            if (count == 0) return new JArray { };

            var query = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, domainCenterColl, fieldStr, pageSize, pageNum, sortStr, findStr);

            JObject[] res = new JObject[0];
            if(query != null && query.Count > 0)
            {
                // 
                string[] txids = query.Select(p => p["txid"].ToString()).Distinct().ToArray();
                var findJo = MongoFieldHelper.toFilter(txids, "txid");
                findJo.Add("displayName", "NNSfixedSellingBuy");
                findStr = findJo.ToString();
                fieldStr = new JObject() { { "price", 1 }, { "txid", 1 } }.ToString();
                var priceRes = mh.GetDataWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, NNSfixedSellingColl, fieldStr, findStr);
                Dictionary<string, string> priceDict = null;
                if(priceRes != null && priceRes.Count > 0)
                {
                    priceDict = priceRes.ToDictionary(k => k["txid"].ToString(), v => v["price"].ToString());
                }
                //
                long[] indexs = query.Select(p => long.Parse(p["blockindex"].ToString())).Distinct().ToArray();
                findStr = MongoFieldHelper.toFilter(indexs, "index").ToString();
                fieldStr = new JObject() { { "time", 1 }, { "index", 1 } }.ToString();
                var timeRes = mh.GetDataWithField(Block_mongodbConnStr, Block_mongodbDatabase, "block", fieldStr, findStr);
                Dictionary<string, long> timeDict = null;
                if (timeRes!= null && timeRes.Count >0)
                {
                    timeDict = timeRes.ToDictionary(k => k["index"].ToString(), v=>long.Parse(v["time"].ToString()));
                }

                res = query.Select(p =>
                {
                    JObject jo = (JObject)p;
                    string price = "0";
                    string txid = jo["txid"].ToString();
                    if (priceDict != null && priceDict.TryGetValue(txid, out string priceVal))
                    {
                        price = priceVal;
                    }
                    jo.Add("price", price);
                    jo.Remove("txid");
                    long time = 0;
                    string index = jo["blockindex"].ToString();
                    if (timeDict != null && timeDict.TryGetValue(index, out long timeVal))
                    {
                        time = timeVal;
                    }
                    jo.Add("time", time);
                    jo.Remove("blockindex");
                    return jo;
                }).ToArray();

            }
            return new JArray() { new JObject() { { "count", count }, { "list", new JArray { res } } } };
        }

        public bool hasNNfixedSelling(string domain, long blockindex, out string price)
        {
            string findStr = new JObject() { { "fullDomain", domain.ToLower() }, { "blockindex", new JObject() { { "$gte", blockindex } } } }.ToString();
            string sortStr = new JObject() { { "blockindex", -1 } }.ToString();
            string fieldStr = new JObject() { { "state", 0 } }.ToString();
            var query = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, NNSfixedSellingColl, fieldStr, 1, 1, sortStr, findStr);
            if (query != null && query.Count > 0)
            {
                string displayName = query[0]["displayName"].ToString();
                if (displayName == "NNSfixedSellingLaunched")
                {
                    price = query[0]["price"].ToString();
                    return true;
                }
            }
            price = "0";
            return false;
        }

        public JArray getDomainInfo(string fulldomain)
        {
            fulldomain = fulldomain.ToLower();
            string namehash = DomainHelper.nameHashFull(fulldomain);
            string findStr = new JObject() { {"namehash", namehash } }.ToString();
            string fieldStr = new JObject() { {"_id",0 },{"owner",1 }, { "TTL", 1 } }.ToString();
            string sortStr = new JObject() { {"blockindex",-1 } }.ToString();
            JArray res = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, domainCenterColl, fieldStr, 1,1, sortStr, findStr);
            return res;
        }

        private JArray format(JArray res, bool isSearch = false)
        {
            if(res == null || res.Count == 0)
            {
                return new JArray() { };
            }
            long nowtime = TimeHelper.GetTimeStamp();
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
                    if(isSearch)
                    {
                        var rr = getDomainInfo(fulldoamin);
                        if(rr != null && rr.Count > 0)
                        {
                            if(p["auctionState"].ToString() == "0401" || p["auctionState"].ToString() == "0601")
                            {

                                JObject resJo = new JObject() {
                                    {"auctionId", p["auctionId"] },
                                    {"fulldomain", p["fulldomain"] },
                                    {"owner", rr[0]["owner"] },
                                    {"ttl", rr[0]["TTL"] }
                                };
                                string price;
                                if(hasNNfixedSelling(p["fulldomain"].ToString(), long.Parse(p["startTime"]["blockindex"].ToString()), out price))
                                {
                                    resJo.Remove("auctionState");
                                    resJo.Add("auctionState", "0901");
                                    resJo.Add("price", price);
                                    return resJo;
                                }
                                resJo.Add("price", "0");
                                return resJo;
                                /*
                                return new JObject() {
                                    {"auctionId", p["auctionId"] },
                                    {"fulldomain", p["fulldomain"] },
                                    {"owner", rr[0]["owner"] },
                                    {"ttl", rr[0]["TTL"] }
                                };
                                */
                            }
                            ttl = long.Parse(rr[0]["TTL"].ToString());
                            jo.Remove("ttl");
                            jo.Add("ttl", ttl);
                            if(ttl > nowtime)
                            {
                                // 过期域名续约后状态需更新为0401
                                jo.Remove("auctionState");
                                jo.Add("auctionState", "0401");
                            }
                        }
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
            return format(res, true);
        }
        public JArray getAuctionRes(string fulldomain)
        {
            fulldomain = fulldomain.ToLower();
            // 域名信息:
            // 域名 + 哈希 + 开始时间 + 结束时间 + maxBuyer + maxPrice + auctionState + 开标块
            string findStr = new JObject() { { "fulldomain", fulldomain } }.ToString();
            string sortStr = new JObject() { { "startTime.blockindex", -1 } }.ToString();
            string fieldStr = MongoFieldHelper.toReturn(new string[] { "fulldomain", "auctionId", "startTime.blocktime", "endTime.blocktime", "lastTime.blocktime", "maxBuyer", "maxPrice", "auctionState", "startTime.blockindex", "ttl" }).ToString();
            JArray res = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, auctionStateColl, fieldStr, 1, 1, sortStr, findStr);
            res = format(res);

            if(res != null && res.Count > 0 && (res[0]["auctionState"].ToString() == "0401" || res[0]["auctionState"].ToString() == "0601"))
            {
                findStr = new JObject() { { "namehash", DomainHelper.nameHashFullDomain(fulldomain) } }.ToString();
                sortStr = new JObject() { { "blockindex", -1 } }.ToString();
                fieldStr = new JObject() { { "TTL", 1 } }.ToString();
                var ttlRes = mh.GetDataPagesWithField(Notify_mongodbConnStr, Notify_mongodbDatabase, domainCenterColl, fieldStr, 1, 1, sortStr, findStr);
                if (ttlRes != null && ttlRes.Count > 0)
                {
                    if ((long)ttlRes[0]["TTL"] > TimeHelper.GetTimeStamp())
                    {
                        var jo = (JObject)res[0];
                        jo.Remove("auctionState");
                        jo.Add("auctionState", "0401");
                        jo.Remove("ttl");
                        jo.Add("ttl", ttlRes[0]["TTL"]);
                        res = new JArray() { jo };
                    }
                }
            }
            return res;
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
