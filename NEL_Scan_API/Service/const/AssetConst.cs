using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NEL_Scan_API.Service.constant
{
    public class AssetConst
    {
        public static string id_neo = "0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b";
        public static string id_gas = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";

        public static string id_neo_name = "小蚁股";
        public static string id_neo_name_ = "AntShare";
        public static string id_gas_name = "小蚁币";
        public static string id_gas_name_ = "AntCoin";
        public static string id_neo_nick = "NEO";
        public static string id_gas_nick = "GAS";

        private static Dictionary<string, string> dict = new Dictionary<string, string>
        {
            { "0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b","NEO" },
            { "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7","GAS" },
            { "0x0c092117b4ba47b81001712425e6e7f760a637695eaf23741ba335925b195ecd","TestCoin" },
            { "0xb426d50907c2b1ff91a8d5c8f1da3bea77e79ada05885719130d99cabae697c0","KAC" },
            { "0xbbb7a08e52a5242079487fead6753dd038d41197e04e342b6f7b7358936551ea","申一股份" },
            { "0x2dbd5d6be093f6bdd7e59d1faedfd2656422aaf749719903e8dab412b4349e81","CNY" },
            { "0x21ef3190e1c8fb9986d63ab98eed4937a3f095e2ed9fd05220a9047c88561319","测试股" },
            { "0xbeb6f821b9141269f06ee5205531a13777be727ec005c53334f2ea82585426fb","量子积分" },
            { "0xab84419d6a391b50400dc6f5ab63528ea8ecb32b81addfb4c7f8afe44be6c1ac","量子股份" },
            { "0xbb9d95d887558e73a27af025f2b7b93d6c9ac9a7d5c811118afb2d6e28599fbe","注册代币类资产" },
            { "0xe13440dccae716e16fc01adb3c96169d2d08d16581cad0ced0b4e193c472eac1","申一币" },
            { "0x4a629db0af0d9c7ee0e11f4f4894765f5ab2579bcc8b4a203e4c6814a9784f00","NEOVERSION" },
            { "0x252a904aac6c2205e47968e407ce64531218e637e2ac073266da06d68fce71d8","测试" },
            { "0x67817fa4003996bf9ecf2a55aaa7eb5ee08a8514cf8cbe9065c3e5404f2c1adc","QLC_T2" },
            { "0x3925d9b14534e6d9de298064e7f13ee4caf364ba1a65cc9d7d1d36a074fa89da","ONT" },
            { "0xc12c6ccc5be5235b90822c4feee70645b9d0bac0636b07bd1d68e34ba8804747","NNC" },
            { "0xf7c8259a97826d6bf81cf3f5171a61c30b56762b18ab5f8238d87148be2913e2","NNC" },
            { "0xd04e64bcba325d9550793cfc8eb694f7a49d188c7d2321700b9fc48c852111fc","hextest" },
            { "0x6cd542eb363c6d6f79b73ef2dfc8a846625cf6354efcb477f6137ebfda79a284","XPF" },
            { "0x1ccc97769fdcb4effce4716b4eda7c65930f7e2da2d06587de025f8ca8d938d3","NKN" },
            { "0x0d01ba4e8d4d01cb4b0aef49740514726bd2e3200e6fbb126caa3fe321ca553e","takokora" },
            { "0x22960ea6004fc6f654fc85e39d6913085dd3f923be932476b56994adaceb37a0","TESTBEAR" },
            { "0x87e07402c17875ed0a5dde4e5792542bc6aa41eb6deee5e53cc3153ded2a923e","XEON" },
            { "0x438777dc0b64f9fcb85031d17bfa8f0da53b0c9b1f3867a86fd738100d5da21a","LivinGCoin" },
            { "0xd67fa588fff2a89e174fc4d74f68f7b9464f6f0c56c848ae0e59c480dac6ec7e","NEO" },
            { "0x500b5c27ff9fa1b5a58da4a1c8f0a8bd0aea0d8e665d6f7184e93a99501c4c12","USDT" },
            { "0x6464a9a7a7a421f06aaed7adc58b8078766aace9ab93e36eff4cd57f886bc70d","asToken" },
            //
            { "0x439af8273fbe25fec2f5f2066679e82314fe0776d52a8c1c87e863bd831ced7d","Hello AntShares Mainnet" },
            { "0x7ed4d563277f54a1535f4406e4826882287fb74d06a1a53e76d3d94d9b3b946a","宝贝评级" },
            { "0x025d82f7b00a9ff1cfe709abe3c4741a105d067178e645bc3ebad9bc79af47d4","TestCoin" },
            { "0x308b0b336e2ed3d718ef92693b70d30b4fe20821265e8e76aecd04a643d0d7fa","明星资本" },
            { "0x9b63fa15ed58e93339483619175064ecadbbe953436a22c31c0053dedca99833","未来研究院" },
            { "0xdd977e41a4e9d5166003578271f191aae9de5fc2de90e966c8d19286e37fa1e1","橙诺" },
            { "0x459ef82138f528c5ff79dd67dcfe293e6a348e447ed8f6bce5b79dded2e63409","赏金（SJ-Money)" },
            { "0x7f48028c38117ac9e42c8e1f6f06ae027cdbb904eaf1a0bdc30c9d81694e045c","无忧宝" },
            { "0x1b504c5fb070aaca3d57c42b5297d811fe6f5a0c5d4cd4496261417cf99013a5","量子股份" },
            { "0x0ab0032ade19975183c4ac90854f1f3c3fc535199831e7d8f018dabb2f35081f","量子积分" },
            { "0x07de511668e6ecc90973d713451c831d625eca229242d34debf16afa12efc1c1","开拍学园币（KAC）" },
            { "0xa52e3e99b6c2dd2312a94c635c050b4c2bc2485fcb924eecb615852bd534a63f","申一币" },
            { "0x30e9636bc249f288139651d60f67c110c3ca4c3dd30ddfa3cbcec7bb13f14fd4","申一股份" },
        };
        public static string getAssetName(string assetHash)
        {
            if (!assetHash.StartsWith("0x")) assetHash = "0x" + assetHash;
            if (dict.ContainsKey(assetHash)) return dict.GetValueOrDefault(assetHash);
            return "nil";
        }
    }
}
