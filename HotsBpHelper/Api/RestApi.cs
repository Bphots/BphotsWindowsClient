using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HotsBpHelper.Api.Model;
using HotsBpHelper.Api.Security;
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
            _securityProvider.SetServerTimestamp(GetTimestamp());
        }

        private RestRequest CreateRequest(string method, IList<Tuple<string, string>> parameters)
        {
            var sp = _securityProvider.CaculateSecurityParameter(parameters);

            parameters.Add(Tuple.Create("timestamp", sp.Timestamp));
            parameters.Add(Tuple.Create("client_patch", sp.Patch));

            string urlParam = string.Join("&", parameters.Select(tuple => $"{tuple.Item1}={tuple.Item2}"));
            /*调试服务器回传信息用
            string u = "https://www.bphots.com/bp_helper/" + method + "?" + urlParam + "&nonce=" + sp.Nonce + "&sign=" + sp.Sign;
            if (method== "get/inform")
                System.Diagnostics.Process.Start(u);
            System.Threading.Thread.Sleep(1000);
            */
            var request = new RestRequest($"{method}?{urlParam}&nonce={sp.Nonce}&sign={sp.Sign}")
            {
                OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; }
            };
            return request;
        }

        private RestRequest CreateRequest(string method)
        {
            var request = new RestRequest(method)
            {
                OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; }
            };
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
                        request.AddParameter("AccountSi`    1   d", _accountSid, ParameterType.UrlSegment); // used on every request
            */
            var response = client.Execute<T>(request);
            EnsureNotErrorResponse(response);
            return response.Data;
        }

        public async Task<List<RemoteFileInfo>> GetRemoteFileListAsync(string url)
        {
            var request = CreateRequest(url, new List<Tuple<string, string>>());
            return await ExecuteAsync<List<RemoteFileInfo>>(request);
        }

        public byte[] DownloadFile(string url)
        {
            using (var client = new WebClient())
            {
                return client.DownloadData(url);
            }
        }

        public List<HeroInfo> GetHeroList(string language)
        {
            var request = CreateRequest("get/herolist",
                new List<Tuple<string, string>>
                {
                    Tuple.Create("lang", language)
                });
            
            return Execute<List<HeroInfo>>(request);
        }

        public List<MapInfo> GetMapList(string language)
        {
            var request = CreateRequest("get/maplist",
                new List<Tuple<string, string>>
                {
                    Tuple.Create("lang", language)
                });

            return Execute<List<MapInfo>>(request);
        }

        public double GetTimestamp()
        {
            var request = CreateRequest("get/timestamp");
            return Execute<double>(request);
        }

        public List<BroadcastInfo> GetBroadcastInfo(string mode,string lang)
        {
            var request = CreateRequest("get/inform",
                new List<Tuple<string, string>>
                {
                    Tuple.Create("mode", mode),
                    Tuple.Create("lang", lang),
                });

            return Execute<List<BroadcastInfo>>(request);
        }
    }
}