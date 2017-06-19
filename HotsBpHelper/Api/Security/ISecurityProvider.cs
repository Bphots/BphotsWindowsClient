using System;
using System.Collections.Generic;

namespace HotsBpHelper.Api.Security
{
    public interface ISecurityProvider
    {
        SecurityParameter CaculateSecurityParameter(IList<Tuple<string, string>> parameters);
    }
}