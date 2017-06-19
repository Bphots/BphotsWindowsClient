using System;
using System.Collections.Generic;
using System.Linq;
using HotsBpHelper.Utils;
using RestSharp;

namespace HotsBpHelper.Api.Security
{
    public class SecurityProvider : ISecurityProvider
    {
        public SecurityParameter CaculateSecurityParameter(IDictionary<string, string> dictParam)
        {
            var sp = new SecurityParameter();
            var strParams = dictParam.OrderBy(kv => kv.Key).Select(kv => $"\"{kv.Key}\":\"{kv.Value}\"");
            string param = "{" + string.Join(",", strParams) + "}";
            sp.Timestamp = ((int)DateTime.Now.ToUnixTimestamp()).ToString();
            sp.Patch = Const.PATCH;
            sp.Nonce = Guid.NewGuid().ToString().Substring(0, 8);
            sp.Sign = Md5Util.CaculateStringMd5($"{Const.KEY}-{sp.Timestamp}-{sp.Patch}-{sp.Nonce}-{param}");
            return sp;
        }
    }
}