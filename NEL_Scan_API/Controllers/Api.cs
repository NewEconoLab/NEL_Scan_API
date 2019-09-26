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
        private DomainService domainService;
        private BlockService blockService;
        private NNSDomainCreditService nnsDomainCrediteService;
        private DaoService daoService;
        private ContractService contractService;

        private mongoHelper mh = new mongoHelper();

        private static Api testApi = new Api("testnet");
        private static Api mainApi = new Api("mainnet");
        public static Api getTestApi() { return testApi; }
        public static Api getMainApi() { return mainApi; }
        private Monitor monitor;

        public Api(string node)
        {
            netnode = node;
            switch (netnode)
            {
                case "testnet":
                    contractService = new ContractService
                    {
                        mh = mh,
                        Block_mongodbConnStr = mh.block_mongodbConnStr_testnet,
                        Block_mongodbDatabase = mh.block_mongodbDatabase_testnet,
                        Notify_mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        Notify_mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        Analysis_mongodbConnStr = mh.analy_mongodbConnStr_testnet,
                        Analysis_mongodbDatabase = mh.analy_mongodbDatabase_testnet
                    };
                    daoService = new DaoService
                    {
                        mh = mh,
                        mongodbConnStr = mh.dao_mongodbConnStr_testnet,
                        mongodbDatabase = mh.dao_mongodbDatabase_testnet
                    };
                    nnsDomainCrediteService = new NNSDomainCreditService
                    {
                        mh = mh,
                        mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                    };
                    blockService = new BlockService
                    {
                        mh = mh,
                        Block_mongodbConnStr = mh.block_mongodbConnStr_testnet,
                        Block_mongodbDatabase = mh.block_mongodbDatabase_testnet,
                    };
                    analyService = new AnalyService
                    {
                        mh = mh,
                        block_mongodbConnStr = mh.block_mongodbConnStr_testnet,
                        block_mongodbDatabase = mh.block_mongodbDatabase_testnet,
                        analy_mongodbConnStr = mh.analy_mongodbConnStr_testnet,
                        analy_mongodbDatabase = mh.analy_mongodbDatabase_testnet,
                    };
                    assetService = new AssetService
                    {
                        mh = mh,
                        mongodbConnStr = mh.block_mongodbConnStr_testnet,
                        mongodbDatabase = mh.block_mongodbDatabase_testnet,
                    };
                    nnsService = new NNSService
                    {
                        mh = mh,
                        block_mongodbConnStr = mh.block_mongodbConnStr_testnet,
                        block_mongodbDatabase = mh.block_mongodbDatabase_testnet,
                        analy_mongodbConnStr = mh.analy_mongodbConnStr_testnet,
                        analy_mongodbDatabase = mh.analy_mongodbDatabase_testnet,
                        notify_mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        notify_mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        //bonusSgas_mongodbConnStr = mh.bonusSgas_mongodbConnStr_testnet,
                        bonusSgas_mongodbConnStr = mh.snapshot_mongodbConnStr_testnet,
                        //bonusSgas_mongodbDatabase = mh.bonusSgas_mongodbDatabase_testnet,
                        bonusSgas_mongodbDatabase = mh.snapshot_mongodbDatabase_testnet,
                        bonusStatisticCol = mh.bonusStatisticCol_testnet,
                        bonusSgasCol = mh.bonusSgasCol_testnet,
                        id_sgas = mh.id_sgas_testnet,
                        auctionStateColl = mh.auctionStateColl_testnet,
                        bonusAddress = mh.bonusAddress_testnet,
                        //nelJsonRPCUrl = mh.nelJsonRPCUrl_testnet
                    };
                    domainService = new DomainService
                    {
                        mh = mh,
                        Block_mongodbConnStr = mh.block_mongodbConnStr_testnet,
                        Block_mongodbDatabase = mh.block_mongodbDatabase_testnet,
                        Notify_mongodbConnStr = mh.notify_mongodbConnStr_testnet,
                        Notify_mongodbDatabase = mh.notify_mongodbDatabase_testnet,
                        auctionStateColl = mh.auctionStateColl_testnet,
                        bonusAddress = mh.bonusAddress_testnet,
                        NNsfixedSellingAddr = mh.NNsfixedSellingAddr_testnet,
                        NNSfixedSellingColl = mh.NNSfixedSellingColl_testnet,
                        domainCenterColl = mh.domainCenterColl_testnet,
                    };
                    break;
                case "mainnet":
                    contractService = new ContractService
                    {
                        mh = mh,
                        Block_mongodbConnStr = mh.block_mongodbConnStr_mainnet,
                        Block_mongodbDatabase = mh.block_mongodbDatabase_mainnet,
                        Notify_mongodbConnStr = mh.notify_mongodbConnStr_mainnet,
                        Notify_mongodbDatabase = mh.notify_mongodbDatabase_mainnet,
                        Analysis_mongodbConnStr = mh.analy_mongodbConnStr_mainnet,
                        Analysis_mongodbDatabase = mh.analy_mongodbDatabase_mainnet
                    };
                    nnsDomainCrediteService = new NNSDomainCreditService
                    {
                        mh = mh,
                        mongodbConnStr = mh.notify_mongodbConnStr_mainnet,
                        mongodbDatabase = mh.notify_mongodbDatabase_mainnet,
                    };
                    blockService = new BlockService
                    {
                        mh = mh,
                        Block_mongodbConnStr = mh.block_mongodbConnStr_mainnet,
                        Block_mongodbDatabase = mh.block_mongodbDatabase_mainnet,
                    };
                    analyService = new AnalyService
                    {
                        mh = mh,
                        block_mongodbConnStr = mh.block_mongodbConnStr_mainnet,
                        block_mongodbDatabase = mh.block_mongodbDatabase_mainnet,
                        analy_mongodbConnStr = mh.analy_mongodbConnStr_mainnet,
                        analy_mongodbDatabase = mh.analy_mongodbDatabase_mainnet,
                    };
                    assetService = new AssetService
                    {
                        mh = mh,
                        mongodbConnStr = mh.block_mongodbConnStr_mainnet,
                        mongodbDatabase = mh.block_mongodbDatabase_mainnet,
                    };
                    nnsService = new NNSService
                    {
                        mh = mh,
                        block_mongodbConnStr = mh.block_mongodbConnStr_mainnet,
                        block_mongodbDatabase = mh.block_mongodbDatabase_mainnet,
                        analy_mongodbConnStr = mh.analy_mongodbConnStr_mainnet,
                        analy_mongodbDatabase = mh.analy_mongodbDatabase_mainnet,
                        notify_mongodbConnStr = mh.notify_mongodbConnStr_mainnet,
                        notify_mongodbDatabase = mh.notify_mongodbDatabase_mainnet,
                        //bonusSgas_mongodbConnStr = mh.bonusSgas_mongodbConnStr_mainnet,
                        bonusSgas_mongodbConnStr = mh.snapshot_mongodbConnStr_mainnet,
                        //bonusSgas_mongodbDatabase = mh.bonusSgas_mongodbDatabase_mainnet,
                        bonusSgas_mongodbDatabase = mh.snapshot_mongodbDatabase_mainnet,
                        bonusStatisticCol = mh.bonusStatisticCol_mainnet,
                        bonusSgasCol = mh.bonusSgasCol_mainnet,
                        id_sgas = mh.id_sgas_mainnet,
                        auctionStateColl = mh.auctionStateColl_mainnet,
                        bonusAddress = mh.bonusAddress_mainnet,
                        //nelJsonRPCUrl = mh.nelJsonRPCUrl_mainnet
                    };
                    domainService = new DomainService
                    {
                        mh = mh,
                        Block_mongodbConnStr = mh.block_mongodbConnStr_mainnet,
                        Block_mongodbDatabase = mh.block_mongodbDatabase_mainnet,
                        Notify_mongodbConnStr = mh.notify_mongodbConnStr_mainnet,
                        Notify_mongodbDatabase = mh.notify_mongodbDatabase_mainnet,
                        auctionStateColl = mh.auctionStateColl_mainnet,
                        bonusAddress = mh.bonusAddress_mainnet,
                        NNsfixedSellingAddr = mh.NNsfixedSellingAddr_mainnet,
                        NNSfixedSellingColl = mh.NNSfixedSellingColl_mainnet,
                        domainCenterColl = mh.domainCenterColl_mainnet,
                    };
                    break;
            }

            initMonitor();
        }

        public object getRes(JsonRPCrequest req, string reqAddr)
        {
            JArray result = null;
            try
            {
                point(req.method);
                switch (req.method)
                {
                    // 获取合约信息
                    case "getContractNep5Tx":
                        result = contractService.getContractNep5Tx(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        break;
                    // 获取合约信息
                    case "getContractCallTx":
                        result = contractService.getContractCallTx(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        break;
                    // 获取合约信息
                    case "getContractInfo":
                        result = contractService.getContractInfo(req.@params[0].ToString());
                        break;
                    
                    // 获取服务列表
                    case "getServiceList":
                        result = daoService.getServiceList(int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()));
                        break;
                    case "getUserInfo":
                        result = daoService.getUserInfo(req.@params[0].ToString());
                        break;
                    case "getHashInfoByVoteHash":
                        result = daoService.getHashInfoByVoteHash(req.@params[0].ToString());
                        break;
                    // 获取治理信息列表(治理)
                    case "getVoteTxHistList":
                        result = daoService.getVoteTxHistList(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        break;
                    // 获取治理信息(治理)
                    case "getVoteInfo":
                        result = daoService.getVoteInfo(req.@params[0].ToString());
                        break;
                    // 获取项目交易历史列表(众筹)
                    case "getProjTxHistList":
                        if (req.@params.Length > 3)
                        {
                            result = daoService.getProjTxHistList(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), req.@params[3].ToString());
                            break;
                        }
                        result = daoService.getProjTxHistList(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        break;
                    // 获取项目信息(众筹)
                    case "getProjInfo":
                        result = daoService.getProjInfo(req.@params[0].ToString());
                        break;
                    // 存储治理信息(治理)
                    case "storeVoteInfo":
                        result = daoService.storeVoteInfo(req.@params[0].ToString(), req.@params[1].ToString(), req.@params[2].ToString(), req.@params[3].ToString(), req.@params[4].ToString(), req.@params[5].ToString(), req.@params[6].ToString(), req.@params[7].ToString());
                        break;
                    // 存储项目信息(众筹)
                    case "storeProjInfo":
                        result = daoService.storeProjInfo(req.@params[0].ToString(), req.@params[1].ToString(), req.@params[2].ToString(), req.@params[3].ToString(), req.@params[4].ToString(), req.@params[5].ToString(), req.@params[5].ToString());
                        break;
                    // .....
                    case "getNep5TxlistByAddress":
                        if(req.@params.Length < 3)
                        {
                            result = blockService.getNep5TxlistByAddress(req.@params[0].ToString());
                        } else
                        {
                            result = blockService.getNep5TxlistByAddress(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        }
                        break;
                    case "getNep5Txlist":
                        if (req.@params.Length < 2)
                        {
                            result = blockService.getNep5Txlist();
                        } else
                        {
                            result = blockService.getNep5Txlist(int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()));
                        }
                        break;
                    //
                    case "getMappingDomain":
                        result = nnsDomainCrediteService.getMappingDomain(req.@params[0].ToString());
                        break;
                    // 
                    case "getNNSFixedSellingList":
                        if (req.@params.Length < 1)
                        {
                            result = nnsService.getNNSFixedSellingList();
                        }
                        else if (req.@params.Length < 3)
                        {
                            result = nnsService.getNNSFixedSellingList(req.@params[0].ToString(), req.@params[1].ToString());
                        }
                        else if(req.@params.Length < 5)
                        {
                            result = nnsService.getNNSFixedSellingList(req.@params[0].ToString(), req.@params[1].ToString(), int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()));
                        } else
                        {
                            result = nnsService.getNNSFixedSellingList(req.@params[0].ToString(), req.@params[1].ToString(), int.Parse(req.@params[2].ToString()), int.Parse(req.@params[3].ToString()), req.@params[4].ToString());
                        }
                        break;
                    // 获取域名流转历史
                    case "getDomainTransferHist":
                        if (req.@params.Length < 3)
                        {
                            result = domainService.getDomainTransferAndSellingInfo(req.@params[0].ToString());
                        }
                        else
                        {
                            result = domainService.getDomainTransferAndSellingInfo(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        }
                        break;
                    case "gettransactionlist":
                        if (req.@params.Length < 2)
                        {
                            result = blockService.gettransactionlist();
                        } else if(req.@params.Length < 3)
                        {
                            result = blockService.gettransactionlist(int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()));
                        } else
                        {
                            result = blockService.gettransactionlist(int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()), req.@params[2].ToString());
                        }
                        break;
                    case "getnep5transferinfo":
                        break;
                    case "getdomaininfo":
                        result = domainService.getDomainInfo(req.@params[0].ToString());
                        break;
                    case "getauctioninfoTx":
                        if (req.@params.Length < 3)
                        {
                            result = domainService.getAuctionInfoTx(req.@params[0].ToString());
                        } else
                        {
                            result = domainService.getAuctionInfoTx(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        }
                            
                        break;
                    case "getauctioninfoRank":
                        if (req.@params.Length < 3)
                        {
                            result = domainService.getAuctionInfoRank(req.@params[0].ToString());
                        } else
                        {
                            result = domainService.getAuctionInfoRank(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        }
                        break;
                    case "getauctioninfo":
                        result = domainService.getAuctionInfo(req.@params[0].ToString());
                        break;
                    case "getauctionres":
                        result = domainService.getAuctionRes(req.@params[0].ToString());
                        break;
                    case "searchbydomain":
                        result = domainService.searchByDomain(req.@params[0].ToString());
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
                    case "getauctingdomainbymaxprice":
                        if (req.@params.Length < 2)
                        {
                            result = nnsService.getAuctingDomainListByMaxPrice();
                        }
                        else
                        {
                            result = nnsService.getAuctingDomainListByMaxPrice(int.Parse(req.@params[0].ToString()), int.Parse(req.@params[1].ToString()));
                        }
                        break;
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
                    // 
                    case "getaddresstxs":
                        result = analyService.getAddressTxsNew(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()));
                        break;
                    case "getrankbyasset":
                        result = analyService.getRankByAsset(req.@params[0].ToString(), int.Parse(req.@params[1].ToString()), int.Parse(req.@params[2].ToString()), netnode);
                        break;
                    case "getrankbyassetcount":
                        result = analyService.getRankByAssetCount(req.@params[0].ToString(), netnode);
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

        private void initMonitor()
        {
            string startMonitorFlag = mh.startMonitorFlag ;
            if (startMonitorFlag == "1")
            {
                monitor = new Monitor();
            }
        }
        private void point(string method)
        {
            if(monitor != null)
            {
                monitor.point(netnode, method);
            }
        }
    }
}

