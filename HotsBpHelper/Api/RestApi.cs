using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HotsBpHelper.Api.Model;
using HotsBpHelper.Api.Security;
using HotsBpHelper.Uploader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Deserializers;
using Stylet;

namespace HotsBpHelper.Api
{
    public class RestApi : IRestApi
    {
        private readonly ISecurityProvider _securityProvider;

        public RestApi(ISecurityProvider securityProvider, IEventAggregator eventAggregator)
        {
            _securityProvider = securityProvider;
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

        public void DownloadFileAsync(string url, DownloadProgressChangedEventHandler downloadProgressChanged,
            DownloadDataCompletedEventHandler downloadCompleted)
        {
            var client = new WebClient();
            client.DownloadProgressChanged += downloadProgressChanged;
            client.DownloadDataCompleted += downloadCompleted;
            client.DownloadDataAsync(new Uri(url));
        }
        
        public List<LobbyHeroInfo> GetLobbyHeroList(string language)
        {
            var request = CreateRequest("get/herolist/lobby",
                new List<Tuple<string, string>>
                {
                    Tuple.Create("lang", language)
                });

            return Execute<List<LobbyHeroInfo>>(request);
        }
        
        public async Task<double> GetTimestamp()
        {
            var request = CreateRequest("get/timestamp");
            return await Task.Run(() => Execute<double>(request)).ConfigureAwait(false);
        }

        public async Task<FingerPrintStatusCollection> CheckDuplicatesAsync(IEnumerable<ReplayIdentity> replayIdentities)
        {
            var fileJson = JsonConvert.SerializeObject(replayIdentities);
            var fileParam = new List<Tuple<string, string>>
            {
                Tuple.Create("files", fileJson)
            };

            var request = CreateRequest("check", fileParam);
            return await ExecuteWeekAsync<FingerPrintStatusCollection>(request);
        }

        public async Task<int> GetMinimalBuild()
        {
            var request = CreateRequest("min-build", new List<Tuple<string, string>>());
            return await ExecuteWeekAsync<int>(request);
        }

        public async Task<UploadStatus> UploadReplay(string file)
        {
            var url = GetSignedUrl(new List<Tuple<string, string>>());
            using (var client = new WebClient())
            {
                var bytes = await client.UploadFileTaskAsync($"{Const.WEB_API_WEEK_ROOT}upload?{url}", file);
                var response = Encoding.UTF8.GetString(bytes);
                var responseItem = JsonConvert.DeserializeObject<GenericResponse>(response);
                return responseItem.Success ? UploadStatus.Success : UploadStatus.UploadError;
            }
        }

        public Dictionary<int, HeroInfoV2> GetHeroListV2()
        {
            var request = CreateRequest("get/herolist/v2",
               new List<Tuple<string, string>>());

            return Execute<Dictionary<int, HeroInfoV2>>(request);
        }

        public Dictionary<string, MapInfoV2> GetMapListV2()
        {
            var request = CreateRequest("get/maplist/v2",
               new List<Tuple<string, string>>());

            return Execute<Dictionary<string, MapInfoV2>>(request);
        }

        public List<BroadcastInfo> GetBroadcastInfo(string mode, string lang)
        {
            var request = CreateRequest("get/inform",
                new List<Tuple<string, string>>
                {
                    Tuple.Create("mode", mode),
                    Tuple.Create("lang", lang)
                });

            return Execute<List<BroadcastInfo>>(request);
        }

        private RestRequest CreateRequest(string method, IList<Tuple<string, string>> parameters)
        {
            var url = GetSignedUrl(parameters);
            /*调试服务器回传信息用
            string u = "https://www.bphots.com/bp_helper/" + method + "?" + urlParam + "&nonce=" + sp.Nonce + "&sign=" + sp.Sign;
            if (method== "get/inform")
                System.Diagnostics.Process.Start(u);
            System.Threading.Thread.Sleep(1000);
            */
            var request = new RestRequest($"{method}?{url}")
            {
                OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; }
            };
            return request;
        }

        private string GetSignedUrl(IList<Tuple<string, string>> parameters)
        {
            var sp = _securityProvider.CaculateSecurityParameter(parameters);

            parameters.Add(Tuple.Create("timestamp", sp.Timestamp));
            parameters.Add(Tuple.Create("client_patch", sp.Patch));

            var urlParam = string.Join("&", parameters.Select(tuple => $"{tuple.Item1}={tuple.Item2}"));

            var url = $"{urlParam}&nonce={sp.Nonce}&sign={sp.Sign}";
            return url;
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

        private async Task<T> ExecuteWeekAsync<T>(RestRequest request) where T : new()
        {
            var client = new RestClient(Const.WEB_API_WEEK_ROOT);
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
    }
}