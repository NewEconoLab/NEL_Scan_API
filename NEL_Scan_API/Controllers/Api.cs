using System;
using Newtonsoft.Json.Linq;
using NEL_Scan_API.Service;
using NEL_Scan_API.RPC;
using NEL_Scan_API.lib;

namespace NEL_Scan_API.Controllers
{
    public class Api
    {
        private string netnode { get; set; }
        
        private AnalyService analyService;
        private AssetService assetService;
        private NNSService nnsService;
        private CommonService commonService;

        private mongoHelper mh = new mongoHelper();

        private static Api testApi = new Api("testnet");
        private static Api mainApi = new Api("mainnet");
        public static Api getTestApi() { return testApi; }
        public static Api getMainApi() { return mainApi; }

        public Api(string node)
        {
            netnode = node;
            switch (netnode)
            {
                case "testnet":
                    analyService = new AnalyService
                    {
                        block_mongodbConnStr = mh.block_mongodbConnStr_testnet,
                        block_mongodbDatabase = mh.block_mongodbDatabase_testnet,
                        analy_mongodbConnStr = mh.analy_mongodbConnStr_testnet,
                        analy_mongodbDatabase = mh.analy_mongodbDatabase_testnet,
                        mh = mh
                    };
                    assetService = new AssetService
                    {
                        mongodbConnStr = mh.block_mongodbConnStr_testnet,
                        mongodbDatabase = mh.block_mongodbDatabase_testnet,
                        mh = mh
                    };
                    nnsService = new NNSService
                    {
                        notify_mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        notify_mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        mh = mh,
                        bonusSgas_mongodbConnStr = mh.bonusSgas_mongodbConnStr_testnet,
                        bonusSgas_mongodbDatabase = mh.bonusSgas_mongodbDatabase_testnet,
                        bonusSgasCol = mh.bonusSgasCol_testnet,
                        id_sgas = mh.id_sgas_testnet,
                        auctionStateColl = mh.auctionStateColl_testnet,
                        bonusAddress = mh.bonusAddress_testnet,
                        nelJsonRPCUrl = mh.nelJsonRPCUrl_testnet
                    };
                    commonService = new CommonService
                    {
                        mh = mh,
                        Block_mongodbConnStr = mh.block_mongodbConnStr_testnet,
                        Block_mongodbDatabase = mh.block_mongodbDatabase_testnet,
                        Notify_mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        Notify_mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        auctionStateColl = mh.auctionStateColl_testnet,
                        bonusAddress = mh.bonusAddress_testnet,
                    };
                    break;
                case "mainnet":
                    analyService = new AnalyService
                    {
                        block_mongodbConnStr = mh.block_mongodbConnStr_mainnet,
                        block_mongodbDatabase = mh.block_mongodbDatabase_mainnet,
                        analy_mongodbConnStr = mh.analy_mongodbConnStr_mainnet,
                        analy_mongodbDatabase = mh.analy_mongodbDatabase_mainnet,
                        mh = mh
                    };
                    assetService = new AssetService
                    {
                        mongodbConnStr = mh.block_mongodbConnStr_mainnet,
                        mongodbDatabase = mh.block_mongodbDatabase_mainnet,
                        mh = mh
                    };
                    nnsService = new NNSService
                    {
                        notify_mongodbConnStr = mh.notify_mongodbConnStr_mainnet,
                        notify_mongodbDatabase = mh.notify_mongodbDatabase_mainnet,
                        mh = mh,
                        bonusSgas_mongodbConnStr = mh.bonusSgas_mongodbConnStr_mainnet,
                        bonusSgas_mongodbDatabase = mh.bonusSgas_mongodbDatabase_mainnet,
                        bonusSgasCol = mh.bonusSgasCol_mainnet,
                        id_sgas = mh.id_sgas_mainnet,
                        auctionStateColl = mh.auctionStateColl_mainnet,
                        bonusAddress = mh.bonusAddress_mainnet,
                        nelJsonRPCUrl = mh.nelJsonRPCUrl_mainnet
                    };
                    commonService = new CommonService
                    {
                        mh = mh,
                        Block_mongodbConnStr = mh.block_mongodbConnStr_mainnet,
                        Block_mongodbDatabase = mh.block_mongodbDatabase_mainnet,
                        Notify_mongodbConnStr = mh.notify_mongodbConnStr_mainnet,
                        Notify_mongodbDatabase = mh.notify_mongodbDatabase_mainnet,
                        auctionStateColl = mh.auctionStateColl_mainnet,
                        bonusAddress = mh.bonusAddress_mainnet,
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
                    case "getauctioninfoTx":
                        if (req.@params.Length < 3)
                        {
                            result = commonService.getAuctionInfoTx(req.@params[0].ToString());
                        } else
                        {
                            result = commonService.getAuctionInfoTx(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        }
                            
                        break;
                    case "getauctioninfoRank":
                        if (req.@params.Length < 3)
                        {
                            result = commonService.getAuctionInfoRank(req.@params[0].ToString());
                        } else
                        {
                            result = commonService.getAuctionInfoRank(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        }
                        break;
                    case "getauctioninfo":
                        result = commonService.getAuctionInfo(req.@params[0].ToString());
                        break;
                    case "searchbydomain":
                        result = commonService.searchByDomain(req.@params[0].ToString());
                        break;
                    // 最具价值域名
                    case "getaucteddomain":
                        if (req.@params.Length < 2)
                        {
                            result = nnsService.getUsedDomainListNew();
                        }
                        else
                        {
                            result = nnsService.getUsedDomainListNew(int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()));
                        }
                        break;
                    // 正在竞拍域名
                    case "getauctingdomain":
                        if (req.@params.Length < 2)
                        {
                            result = nnsService.getAuctingDomainListNew();
                        }
                        else
                        {
                            result = nnsService.getAuctingDomainListNew(int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()));
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
                    case "getaddresstxs":
                        result = analyService.getAddressTxsNew(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        break;
                    case "getrankbyasset":
                        result = analyService.getRankByAsset(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        break;
                    case "getrankbyassetcount":
                        result = analyService.getRankByAssetCount(req.@params[0].ToString());
                        break;
                    
                    // test
                    case "getnodetype":
                        result = new JArray { new JObject { { "nodeType", netnode } } };
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
                Console.WriteLine("errMsg:{0},errStack:{1}", e.Message, e.StackTrace);
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

