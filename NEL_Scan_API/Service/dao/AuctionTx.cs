using MongoDB.Bson;
using System.Collections.Generic;

namespace NEL_Scan_API.Service.dao
{
    class AuctionTx
    {
        public ObjectId _id { get; set; }
        public string auctionId { get; set; }
        public string domain { get; set; }
        public string parenthash { get; set; }
        public string fulldomain { get; set; }
        public long ttl { get; set; }
        public string auctionState { get; set; }
        public AuctionTime startTime { get; set; }
        public string startAddress { get; set; }
        public decimal maxPrice { get; set; }
        public string maxBuyer { get; set; }
        public AuctionTime endTime { get; set; }
        public string endAddress { get; set; }
        public AuctionTime lastTime { get; set; }
        public List<AuctionAddWho> addwholist { get; set; }

    }
    class AuctionAddWho
    {
        public string address { get; set; }
        public decimal totalValue { get; set; }
        public decimal curTotalValue { get; set; }
        public AuctionTime lastTime { get; set; }
        public AuctionTime accountTime { get; set; }
        public AuctionTime getdomainTime { get; set; }
        public List<AuctionAddPrice> addpricelist { get; set; }

    }
    class AuctionAddPrice
    {
        public AuctionTime time { get; set; }
        public decimal value { get; set; }
        public string isEnd { get; set; }
    }
    class AuctionTime
    {
        public long blockindex { get; set; }
        public long blocktime { get; set; }
        public string txid { get; set; }
    }
    class AuctionState
    {
        public const string STATE_START = "0101";   // 开标
        public const string STATE_CONFIRM = "0201"; // 确定期
        public const string STATE_RANDOM = "0301";  // 随机期
        public const string STATE_END = "0401"; // 触发结束、3D/5D到期结束
        public const string STATE_ABORT = "0501";   // 流拍
        public const string STATE_EXPIRED = "0601"; // 过期
    }
}
