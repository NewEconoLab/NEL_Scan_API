using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using NEL_Scan_API.Service;
using NEL_Agency_API.RPC;
using NEL_Scan_API.lib;

namespace NEL_Scan_API.Controllers
{
    public class Api
    {
        private string netnode { get; set; }
        private string mongodbConnStr { get; set; }
        private string mongodbDatabase { get; set; }
        private string notify_mongodbConnStr { get; set; }
        private string notify_mongodbDatabase { get; set; }
        private string nelJsonRPCUrl { get; set; }
        private string mongodbConnStrAtBlock { get; set; }
        private string mongodbDatabaseAtBlock { get; set; }
    
        //private OssFileService ossClient;
        //private AuctionService auctionService;
        //private BonusService bonusService;
        private AssetService assetService;
        private NNSService nnsService;
        private mongoHelper mh = new mongoHelper();

        public Api(string node)
        {
            netnode = node;
            switch (netnode)
            {
                case "testnet":
                    mongodbConnStr = mh.mongodbConnStr_testnet;
                    mongodbDatabase = mh.mongodbDatabase_testnet;
                    notify_mongodbConnStr = mh.notify_mongodbConnStr_testnet;
                    notify_mongodbDatabase = mh.notify_mongodbDatabase_testnet;
                    nelJsonRPCUrl = mh.nelJsonRPCUrl_testnet;
                    mongodbConnStrAtBlock = mh.mongodbConnStrAtBlock_testnet;
                    mongodbDatabaseAtBlock = mh.mongodbDatabaseAtBlock_testnet;
                    assetService = new AssetService
                    {
                        mongodbConnStr = mh.mongodbConnStrAtBlock_testnet,
                        mongodbDatabase = mh.mongodbDatabaseAtBlock_testnet,
                        mh = mh
                    };
                    nnsService = new NNSService
                    {
                        newNotify_mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        newNotify_mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        nnsDomainState = mh.nnsDomainState_testnet,
                        mh = mh,
                    };
                    break;
                case "mainnet":
                    mongodbConnStr = mh.mongodbConnStr_mainnet;
                    mongodbDatabase = mh.mongodbDatabase_mainnet;
                    notify_mongodbConnStr = mh.notify_mongodbConnStr_mainnet;
                    notify_mongodbDatabase = mh.notify_mongodbDatabase_mainnet;
                    nelJsonRPCUrl = mh.nelJsonRPCUrl_mainnet;
                    mongodbConnStrAtBlock = mh.mongodbConnStrAtBlock_mainnet;
                    mongodbDatabaseAtBlock = mh.mongodbDatabaseAtBlock_mainnet;
                    assetService = new AssetService
                    {
                        mongodbConnStr = mh.mongodbConnStrAtBlock_mainnet,
                        mongodbDatabase = mh.mongodbDatabaseAtBlock_mainnet,
                        mh = mh
                    };
                    nnsService = new NNSService
                    {
                        newNotify_mongodbConnStr = mh.notify_mongodbConnStr_mainnet,
                        newNotify_mongodbDatabase = mh.notify_mongodbDatabase_mainnet,
                        nnsDomainState = mh.nnsDomainState_mainnet,
                        mh = mh,
                    };
                    break;
            }
        }

        public object getRes(JsonRPCrequest req, string reqAddr)
        {
            JArray result = null;
            try
            {
                switch (req.method)
                {
                    // 获取域名信息
                    case "getdomaininfo":
                        result = nnsService.getDomain(req.@params[0].ToString());
                        break;
                    // 最具价值域名
                    case "getaucteddomain":
                        if (req.@params.Length < 2)
                        {
                            result = nnsService.getUsedDomainList();
                        }
                        else
                        {
                            result = nnsService.getUsedDomainList(int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()));
                        }
                        break;
                    // 正在竞拍域名
                    case "getauctingdomain":
                        if (req.@params.Length < 2)
                        {
                            result = nnsService.getAuctingDomainList();
                        }
                        else
                        {
                            result = nnsService.getAuctingDomainList(int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()));
                        }
                        break;
                    // statistics(奖金池+已领分红+已使用域名数量+正在竞拍域名数量)
                    case "getstatistics":
                        result = nnsService.getStatistic();
                        break;
                    // 资产名称模糊查询
                    case "fuzzysearchasset":
                        result = assetService.fuzzySearchAsset(req.@params[0].ToString());
                        break;
                    // 根据地址查询txid列表
                    case "gettxidsetbyaddress":
                        string queryTxidSetAddr = "{$or: [{\"from\":\"" + req.@params[0] + "\"}" + "," + "{\"to\":\"" + req.@params[0] + "\"}]}";
                        JArray queryTxidSetRes = mh.GetData(notify_mongodbConnStr, notify_mongodbDatabase, "0x4ac464f84f50d3f902c2f0ca1658bfaa454ddfbf", queryTxidSetAddr);
                        if (queryTxidSetRes == null || queryTxidSetRes.Count() == 0)
                            result = new JArray { };
                        else
                            result = new JArray() { queryTxidSetRes.Select(item => item["txid"].ToString()).ToArray() };
                        break;
                }
                if (result.Count == 0)
                {
                    JsonPRCresponse_Error resE = new JsonPRCresponse_Error(req.id, -1, "No Data", "Data does not exist");

                    return resE;
                }
            }
            catch (Exception e)
            {
                JsonPRCresponse_Error resE = new JsonPRCresponse_Error(req.id, -100, "Parameter Error", e.Message);

                return resE;

            }

            JsonPRCresponse res = new JsonPRCresponse();
            res.jsonrpc = req.jsonrpc;
            res.id = req.id;
            res.result = result;

            return res;
        }
    }
}

