using System;
using System.Collections.Generic;
using System.Linq;
using HotsBpHelper.Utils;
using RestSharp;

namespace HotsBpHelper.Api.Security
{
    public class SecurityProvider : ISecurityProvider
    {
        public SecurityParameter CaculateSecurityParameter(IList<Tuple<string, string>> parameters)
        {
            var sp = new SecurityParameter();
            var strParams = parameters.OrderBy(tuple => tuple.Item1).Select(tuple => $"\"{tuple.Item1}\":\"{tuple.Item2}\"");
            string param = "{" + string.Join(",", strParams) + "}";
            sp.Timestamp = ((int)DateTime.Now.ToUnixTimestamp()).ToString();
            sp.Patch = Const.PATCH;
            sp.Nonce = Guid.NewGuid().ToString().Substring(0, 8);
            sp.Sign = Md5Util.CaculateStringMd5($"{Const.KEY}-{sp.Timestamp}-{sp.Patch}-{sp.Nonce}-{param}");
            return sp;
        }
    }
}