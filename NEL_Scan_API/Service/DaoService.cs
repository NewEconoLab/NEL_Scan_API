using NEL_Scan_API.lib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NEL_Scan_API.Service
{
    public class DaoService
    {
        public mongoHelper mh { get; set; }
        public string mongodbConnStr { get; set; }
        public string mongodbDatabase { get; set; }

        private string userInfoCol = "futureDao_UserInfo";
        private string projInfoCol = "futureDao_ProjInfo";
        private string voteInfoCol = "futureDao_VoteInfo";
        private string ethPriceStateCol = "ethPriceState";
        private string ethVoteStateCol = "ethVoteState";

        // 发布项目
        public JArray storeProjInfo(string txid, string address, string creator, string projName, string projDetail, string projTeam, string revengePlan)
        {
            string findStr = new JObject { { "txid", txid } }.ToString();
            if (mh.GetDataCount(mongodbConnStr, mongodbDatabase, projInfoCol, findStr) == 0)
            {
                var newdata = new JObject {
                    { "hash", ""},
                    { "voteHash", ""},
                    { "txid", txid},
                    { "address", address},
                    { "creator", creator},
                    { "projName", projName},
                    { "projDetail", projDetail},
                    { "projTeam", projTeam},
                    { "revengePlan", revengePlan},
                    { "time", TimeHelper.GetTimeStamp()}
                }.ToString();
                mh.PutData(mongodbConnStr, mongodbDatabase, projInfoCol, newdata);
                return new JArray { new JObject { { "res", true } } };
            }
            return new JArray { new JObject { { "res", false } } };
        }

        // 发布治理
        public JArray storeVoteInfo(string hash, string voteHash, string txid, string address, string voter, string name, string summary, string detail)
        {
            string findStr = new JObject { { "hash", hash }, { "voteHash", voteHash }, { "address", address }, { "name", name } }.ToString();
            if (mh.GetDataCount(mongodbConnStr, mongodbDatabase, voteInfoCol, findStr) == 0)
            {
                var newdata = new JObject {
                    { "hash", hash},
                    { "voteHash", voteHash},
                    { "txid", txid},
                    { "address", address},
                    { "voter", voter},
                    { "name", name},
                    { "summary", summary},
                    { "detail", detail},
                    { "time", TimeHelper.GetTimeStamp()}
                }.ToString();
                mh.PutData(mongodbConnStr, mongodbDatabase, voteInfoCol, newdata);
                return new JArray { new JObject { { "res", true } } };
            }
            return new JArray { new JObject { { "res", false } } };
        }
        // 投票

        // 显示
        // 显示.交易详情
        public JArray getProjInfo(string hash)
        {
            //发起人/已筹资金/ 目标资金/ 剩余时间 /总参与人数/ 已售出股份数
            string findStr = new JObject { { "hash", hash } }.ToString();
            string fieldStr = new JObject { { "address", 1 },{ "blockindex", 1 },{ "perFrom24h",1} }.ToString();
            var queryRes = mh.GetDataWithField(mongodbConnStr, mongodbDatabase, ethPriceStateCol, fieldStr, findStr);
            int joinCount = 0;
            var perFrom24h = "0";//getFntAmountUtilTodayZeroClockPri(hash).ToString(); 
            if (queryRes != null && queryRes.Count > 0)
            {
                joinCount = queryRes.Select(p => p["address"].ToString()).Distinct().Count();
                perFrom24h = queryRes.OrderByDescending(p => p["blockindex"]).ToArray()[0]["perFrom24h"].ToString();
            }

            var voteHash = "";
            var creator = "";
            var address = "";
            var projName = "";
            var projDetail = "";
            fieldStr = new JObject { { "voteHash", 1 }, { "address", 1 }, { "creator",1},{ "projName",1},{ "projDetail",1}
        }.ToString();
            var subres = mh.GetDataWithField(mongodbConnStr, mongodbDatabase, projInfoCol, fieldStr, findStr);
            if (subres != null && subres.Count > 0)
            {
                voteHash = subres[0]["voteHash"].ToString();
                address = subres[0]["address"].ToString();
                creator = subres[0]["creator"].ToString();
                projName = subres[0]["projName"].ToString();
                projDetail = subres[0]["projDetail"].ToString();
            }

            return new JArray { new JObject { { "voteHash", voteHash }, { "address", address }, { "creator", creator }, { "projName", projName }, { "projDetail", projDetail }, { "joinCount", joinCount }, { "perFrom24h", perFrom24h } } };
        }
        public JArray getProjTxHistList(string hash, int pageNum = 1, int pageSize = 10, string address="")
        {
            //time/txid/height/address/eventName/ethAmount/fndAmount
            //string findStr = new JObject { { "hash", hash } }.ToString();
            var findJo = new JObject { { "hash", hash } };
            if (address != "")
            {
                findJo.Add("address", address.ToLower());
            }
            string findStr = findJo.ToString();
            string sortStr = new JObject { { "blocktime", -1 } }.ToString();
            var queryRes = mh.GetDataPages(mongodbConnStr, mongodbDatabase, ethPriceStateCol, sortStr, pageSize, pageNum, findStr);
            if (queryRes == null || queryRes.Count == 0) return new JArray { };

            var count = mh.GetDataCount(mongodbConnStr, mongodbDatabase, ethPriceStateCol, findStr);

            return new JArray { new JObject { { "count", count }, { "list", queryRes } } };
        }

        // 显示.治理详情
        public JArray getVoteInfo(string hash)
        {
            //发起人/已筹资金/ 治理资金/ 启动时间 /总参与人数/ 已售出股份数
            string findStr = new JObject { { "hash", hash } }.ToString();
            string fieldStr = new JObject { { "address", 1 } }.ToString();
            var queryRes = mh.GetDataWithField(mongodbConnStr, mongodbDatabase, ethPriceStateCol, fieldStr, findStr);
            int joinCount = 0;
            if (queryRes != null && queryRes.Count > 0)
            {
                joinCount = queryRes.Select(p => p["address"].ToString()).Distinct().Count();
            }

            var creator = "";
            var voteHash = "";
            fieldStr = new JObject { { "creator", 1 }, { "voteHash", 1 } }.ToString();
            var subres = mh.GetDataWithField(mongodbConnStr, mongodbDatabase, projInfoCol, fieldStr, findStr);
            if (subres != null && subres.Count > 0)
            {
                creator = subres[0]["creator"].ToString();
                voteHash = subres[0]["voteHash"].ToString();
            }

            return new JArray { new JObject { { "creator", creator }, { "voteHash", voteHash }, { "joinCount", joinCount } } };
        }
        public JArray getVoteTxHistList(string hash, int pageNum = 1, int pageSize = 10)
        {
            //名称/所需时间/地址/状态<投票中>/申请金额/支持数量/反对数量
            string findStr = new JObject { { "hash", hash } }.ToString();
            string sortStr = new JObject { { "startTime", -1 } }.ToString();
            string fieldStr = new JObject { { "_id",0},
                { "hash", 1 },
                { "voteHash", 1 },
                { "proposalIndex", 1 },
                { "proposalName", 1 },
                { "timeConsuming", 1 },
                { "proposer",1 },
                { "proposalState", 1 },
                { "value", 1 },
                { "voteYesCount", 1 },
                { "voteNotCount", 1 }
            }.ToString();
            var queryRes = mh.GetDataPagesWithField(mongodbConnStr, mongodbDatabase, ethVoteStateCol, fieldStr, pageSize, pageNum, sortStr, findStr);
            if (queryRes == null || queryRes.Count == 0) return new JArray { };

            var res = queryRes.Select(p =>
            {
                string voter = "";
                JObject jo = (JObject)p;
                var subfindStr = new JObject {
                    { "hash", jo["hash"].ToString().ToLower() },
                    { "voteHash", jo["voteHash"].ToString().ToLower() },
                    { "name", jo["proposalName"].ToString()}
                }.ToString();
                var subsortStr = new JObject { { "blocktime", -1 } }.ToString();
                var subfieldStr = new JObject { { "voter", 1 } }.ToString();
                var subres = mh.GetDataWithField(mongodbConnStr, mongodbDatabase, voteInfoCol, subfieldStr, subfieldStr);
                if (subres != null && subres.Count > 0) { voter = subres[0]["voter"].ToString(); }
                jo.Add("voter", voter);
                return jo;
            }).ToArray();
            var count = mh.GetDataCount(mongodbConnStr, mongodbDatabase, ethVoteStateCol, findStr);
            return new JArray { new JObject { { "count", count }, { "list", new JArray { res } } } };
        }

        public JArray getHashInfoByVoteHash(string voteHash)
        {
            string findStr = new JObject { { "voteHash", voteHash } }.ToString();
            string fieldStr = new JObject { { "hash", 1 }, { "projName", 1 } }.ToString();
            var queryRes = mh.GetDataWithField(mongodbConnStr, mongodbDatabase, projInfoCol, fieldStr, findStr);
            if (queryRes == null || queryRes.Count == 0) return new JArray { };

            return new JArray { new JObject { { "hash", queryRes[0]["hash"].ToString() }, { "projName", queryRes[0]["projName"].ToString() } } };
        }
        public JArray getUserInfo(string address)
        {
            string findStr = new JObject { { "address", address } }.ToString();
            string fieldStr = new JObject { { "_id", 0 } }.ToString();
            return mh.GetDataWithField(mongodbConnStr, mongodbDatabase, userInfoCol, fieldStr, findStr);
        }


        // 显示.查询服务列表
        public JArray getServiceList(int pageNum = 1, int pageSize = 10)
        {
            string findStr = "{}";
            string sortStr = new JObject { { "time", -1 } }.ToString();
            string fieldStr = new JObject { { "hash", 1 }, { "voteHash", 1 }, { "address", 1 }, { "creator", 1 }, { "projName", 1 }, { "_id", 0 } }.ToString();
            var queryRes = mh.GetDataPagesWithField(mongodbConnStr, mongodbDatabase, projInfoCol, fieldStr, pageSize, pageNum, sortStr, findStr);
            if (queryRes == null || queryRes.Count == 0) return new JArray { };

            var res = queryRes.Select(p =>
            {
                JObject jo = (JObject)p;
                
                var perFrom24h = "0";
                var subfindStr = new JObject { { "hash", jo["hash"].ToString().ToLower() } }.ToString();
                var subsortStr = new JObject { { "blocktime", -1 } }.ToString();
                var subfieldStr = new JObject { { "perFrom24h", 1 } }.ToString();
                var subres = mh.GetDataPagesWithField(mongodbConnStr, mongodbDatabase, ethPriceStateCol, subfieldStr, 1, 1, subsortStr, subfindStr);
                if (subres != null && subres.Count > 0)
                {
                    perFrom24h = subres[0]["perFrom24h"].ToString();
                }
                jo.Add("perFrom24h", perFrom24h);
                //jo.Add("perFrom24h", getFntAmountUtilTodayZeroClockPri(jo["hash"].ToString().ToLower()).ToString());
                return jo;
            }).ToArray();
            var count = mh.GetDataCount(mongodbConnStr, mongodbDatabase, ethPriceStateCol, findStr);
            return new JArray { new JObject { { "count", count }, { "list", new JArray { res } } } };
        }
        private decimal getFntAmountUtilTodayZeroClockPri(string hash)
        {
            string findStr = new JObject { { "hash", hash }, { "blocktime", new JObject { { "$lte", TimeHelper.GetTimeStampZeroBj() } } } }.ToString();
            string fieldStr = new JObject { { "fndAmount", 1 } }.ToString();
            var queryRes = mh.GetDataWithField(mongodbConnStr, mongodbDatabase, ethPriceStateCol, fieldStr, findStr);
            if (queryRes != null && queryRes.Count > 0)
            {
                return queryRes.Sum(p => decimal.Parse(p["fndAmount"].ToString()));
            }
            return 0;
        }
    }
}
