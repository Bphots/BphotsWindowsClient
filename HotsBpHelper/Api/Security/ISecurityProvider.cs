using System.Collections.Generic;

namespace HotsBpHelper.Api.Security
{
    public interface ISecurityProvider
    {
        SecurityParameter CaculateSecurityParameter(IDictionary<string, string> dictParam);
    }
}