using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HotsBpHelper.Api.Model;
using HotsBpHelper.Api.Security;
using HotsBpHelper.Utils;
using NLog;
using RestSharp;
using RestSharp.Deserializers;

namespace HotsBpHelper.Api
{
    public class RestApi : IRestApi
    {
        private readonly ISecurityProvider _securityProvider;

        public RestApi(ISecurityProvider securityProvider)
        {
            _securityProvider = securityProvider;
        }

        private RestRequest CreateRequest(string method, IDictionary<string, string> dictParam, bool returnJson = true)
        {
            var sp = _securityProvider.CaculateSecurityParameter(dictParam);

            dictParam["timestamp"] = sp.Timestamp;
            dictParam["client_patch"] = sp.Patch;

            string urlParam = string.Join("&", dictParam.Select(kv => $"{kv.Key}={kv.Value}"));
            var request = new RestRequest($"{method}?{urlParam}&nonce={sp.Nonce}&sign={sp.Sign}");
            if (returnJson)
            {
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
            }
            return request;
        }

        private async Task<T> ExecuteAsync<T>(RestRequest request) where T : new()
        {
            var client = new RestClient(Const.WEB_API_ROOT);
            /*
                        client.Authenticator = new HttpBasicAuthenticator(_accountSid, _secretKey);
                        request.AddParameter("AccountSid", _accountSid, ParameterType.UrlSegment); // used on every request
            */
            var response = await client.ExecuteTaskAsync<T>(request);
            EnsureNotErrorResponse(response);
            return response.Data;
        }

        private static void EnsureNotErrorResponse<T>(IRestResponse<T> response) where T : new()
        {
            try
            {
                var deserializer = new JsonDeserializer();
                var data = deserializer.Deserialize<Dictionary<string, object>>(response);

                if (data != null && data.ContainsKey("result") && data["result"].ToString() == "failure")
                {
                    throw new Exception(data["error"].ToString());
                }
            }
            catch (InvalidCastException)
            {
                // CONTENT不是error的结构, 无需处理
            }
        }


        private T Execute<T>(RestRequest request) where T : new()
        {
            var client = new RestClient(Const.WEB_API_ROOT);
            /*
                        client.Authenticator = new HttpBasicAuthenticator(_accountSid, _secretKey);
                        request.AddParameter("AccountSid", _accountSid, ParameterType.UrlSegment); // used on every request
            */
            var response = client.Execute<T>(request);
            EnsureNotErrorResponse(response);
            return response.Data;
        }

        public async Task<List<RemoteFileInfo>> GetRemoteFileListAsync()
        {
            var request = CreateRequest("filelist", new Dictionary<string, string>());
            return await ExecuteAsync<List<RemoteFileInfo>>(request);
        }

        public byte[] DownloadFile(string url)
        {
            using (var client = new WebClient())
            {
                return client.DownloadData(url);
            }
        }

        public Dictionary<int, string> GetHeroList(string language)
        {
            var request = CreateRequest("herolist",
                new Dictionary<string, string>()
                {
                    {"lang",language}
                });

            return Execute<Dictionary<int, string>>(request);
        }
    }
}