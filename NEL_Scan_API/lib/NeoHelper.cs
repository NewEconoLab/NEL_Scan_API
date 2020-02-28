using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThinNeo;

namespace NEL_Scan_API.lib
{
    public static class NeoHelper
    {
        public static string address2pubkeyHash(this string address)
        {
            var hash = Helper.GetPublicKeyHashFromAddress(address);
            return hash.ToString();
        }
        public static string pubkeyhash2address(this string hash)
        {
            return Helper.GetAddressFromScriptHash(new Hash160(hash));
        }
    }
}
