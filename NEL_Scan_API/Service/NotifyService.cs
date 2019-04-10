using NEL_Scan_API.lib;
using Newtonsoft.Json.Linq;

namespace NEL_Scan_API.Service
{
    public class NotifyService
    {
        public DBClient dc { set; get; }

        public JArray subscribeDomainNotify(string mail, string code, string domain, string address="")
        {
            if(dc.checkCode(mail, code))
            {
                if(!dc.hasExistSubscriberInfo(mail, domain, address))
                {
                    dc.saveSubscriberInfo(mail, domain, address);
                }
                return getRes(MailResCode.Success); // succ
            }
            return getRes(MailResCode.InvalidCode); // 不合法code
        }
        public JArray getAuthenticationCode(string email)
        {
            // 验证邮箱
            if(!StringHelper.validateEmail(email))
            {
                return getRes(MailResCode.InvalidMail); // 不合法mail
            }
            // 
            if(dc.hasApplyCode(email))
            {
                return getRes(MailResCode.RepeatApply); // 重复申请
            }
            //
            dc.saveApplyCode(email);
            return getRes(MailResCode.Success); // succ

        }
        private JArray getRes(Body res)
        {
            return new JArray() { new JObject() { {"code", res.key }, { "desc", res.val } } };
        }

    }

    class MailResCode
    {
        public static Body Success = new Body { key="0000", val="成功"};
        public static Body InvalidMail = new Body { key = "2001", val = "不合法邮箱" };
        public static Body RepeatApply = new Body { key = "2002", val = "重复申请验证码, 提示：1分钟不能重复申请" };
        public static Body InvalidCode = new Body { key = "2003", val = "不合法验证码" };
    }
    class Body
    {
        public string key { set; get; }
        public string val { set; get; }
    }

    public class DBClient
    {
        private mongoHelper mh { get; set; }
        private string mongodbConnStr { get; set; } 
        private string mongodbDatabase { get; set; }
        private string notifyCodeColl { get; set; }
        private string notifySubsColl { get; set; }

        public static DBClient getInstance(mongoHelper mh, string mongodbConnStr, string mongodbDatabase, string notifyCodeColl, string notifySubsColl)
        {
            return new DBClient
            {
                mh = mh,
                mongodbConnStr = mongodbConnStr,
                mongodbDatabase = mongodbDatabase,
                notifyCodeColl = notifyCodeColl,
                notifySubsColl = notifySubsColl
            };
        }

        public bool hasApplyCode(string mail)
        {
            long time = TimeHelper.GetTimeStamp();

            string findStr = new JObject() {
                {"mail", mail},
                {"time", new JObject(){ {"$gt", time - 60L} } }, // 1分钟内不能重复申请
            }.ToString();
            return mh.GetDataCount(mongodbConnStr, mongodbDatabase, notifyCodeColl, findStr) > 0;
        }
        public bool saveApplyCode(string mail)
        {
            long time = TimeHelper.GetTimeStamp();

            string jdata = new JObject() {
                {"mail", mail },
                {"code", "" },
                {"time", time },
            }.ToString();
            mh.PutData(mongodbConnStr, mongodbDatabase, notifyCodeColl, jdata);
            return true;
        }

        public bool checkCode(string mail, string code)
        {
            long time = TimeHelper.GetTimeStamp();

            string findStr = new JObject() {
                {"mail", mail },
                {"code", code },
                {"time", new JObject(){ {"$gt", time - 120L} } }, // 验证码有效期为2分钟
            }.ToString();
            return mh.GetDataCount(mongodbConnStr, mongodbDatabase, notifyCodeColl, findStr) > 0;
        }

        public void saveSubscriberInfo(string mail, string domain, string address)
        {
            string jdata = new JObject() {
                {"mail", mail },
                {"domain", domain },
                {"address", address },
            }.ToString();
            mh.PutData(mongodbConnStr, mongodbDatabase, notifySubsColl, jdata);
        }

        public bool hasExistSubscriberInfo(string mail, string domain, string address)
        {
            string findStr = new JObject() {
                {"mail", mail },
                {"domain", domain },
            }.ToString();

            return mh.GetDataCount(mongodbConnStr, mongodbDatabase, notifySubsColl, findStr) > 0;
        }
    }
    
}
