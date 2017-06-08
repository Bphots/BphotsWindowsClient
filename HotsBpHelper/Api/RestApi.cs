using System.Collections.Generic;
using System.Threading.Tasks;
using HotsBpHelper.Api.Model;
using RestSharp;

namespace HotsBpHelper.Api
{
    public class RestApi : IRestApi
    {
        private readonly RestClient _client;

        public RestApi()
        {
            _client = new RestClient(Const.WEB_API_ROOT);
        }

        private RestRequest CreateRequest(string resource)
        {
            return new RestRequest(resource)
            {
                OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; }
            };

        }

        public async Task<List<RemoteFileInfo>> GetRemoteFileListAsync()
        {
            var request = CreateRequest("filelist");
            var result = await _client.ExecuteTaskAsync<List<RemoteFileInfo>>(request);
            return result.Data;
        }
    }
}