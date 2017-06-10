using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HotsBpHelper.Api.Model;
using RestSharp;

namespace HotsBpHelper.Api
{
    public class RestApi : IRestApi
    {
        private RestRequest CreateRequest(string resource)
        {
            return new RestRequest(resource)
            {
                OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; }
            };
        }

        private async Task<T> ExecuteAsync<T>(RestRequest request) where T:new ()
        {
            var client = new RestClient(Const.WEB_API_ROOT);
/*
            client.Authenticator = new HttpBasicAuthenticator(_accountSid, _secretKey);
            request.AddParameter("AccountSid", _accountSid, ParameterType.UrlSegment); // used on every request
*/
            var response = await client.ExecuteTaskAsync<T>(request);

            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            return response.Data;
        }

        public async Task<List<RemoteFileInfo>> GetRemoteFileListAsync()
        {
            var request = CreateRequest("filelist");
            return await ExecuteAsync<List<RemoteFileInfo>>(request);
        }

        public byte[] DownloadFile(string filePath)
        {
            var client = new RestClient(Const.WEB_API_ROOT);
            return client.DownloadData(new RestRequest("filedata?path=" + filePath));
        }
    }
}