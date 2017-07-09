using System;
using System.Collections.Generic;

namespace HotsBpHelper.Api.Security
{
    public interface ISecurityProvider
    {
        void SetServerTimestamp(double timestamp);
        SecurityParameter CaculateSecurityParameter(IList<Tuple<string, string>> parameters);
    }
}