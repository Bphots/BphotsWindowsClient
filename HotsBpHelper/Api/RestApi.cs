using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HotsBpHelper.Api.Model;
using HotsBpHelper.Utils;
using RestSharp;
using RestSharp.Deserializers;

namespace HotsBpHelper.Api
{
    public class RestApi : IRestApi
    {
        private RestRequest CreateRequest(string method, IDictionary<string, string> dictParam)
        {
            const string key = "I7@gPm2F4HAcz@ak";

            var strParams = dictParam.OrderBy(kv => kv.Key).Select(kv => $"'{kv.Key}':'{kv.Value}'");
            string param = "{" + string.Join(",", strParams) + "}";
            dictParam["timestamp"] = ((int)DateTime.Now.ToUnixTimestamp()).ToString();
            dictParam["client_patch"] = "17060801";
            string nonce = Guid.NewGuid().ToString().Substring(0, 8);

            string sign = Md5Util.CaculateStringMd5($"{key}-{dictParam["timestamp"]}-{dictParam["client_patch"]}-{nonce}-{param}");
            string urlParam = string.Join("&", dictParam.Select(kv => $"{kv.Key}={kv.Value}"));
            return new RestRequest($"{method}?{urlParam}&nonce={nonce}&sign={sign}")
            {
                OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; }
            };
        }

        private async Task<T> ExecuteAsync<T>(RestRequest request) where T : new()
        {
            var client = new RestClient(Const.WEB_API_ROOT);
            /*
                        client.Authenticator = new HttpBasicAuthenticator(_accountSid, _secretKey);
                        request.AddParameter("AccountSid", _accountSid, ParameterType.UrlSegment); // used on every request
            */
            var response = await client.ExecuteTaskAsync<T>(request);
            var deserializer = new JsonDeserializer();
            var data = deserializer.Deserialize<Dictionary<string, object>>(response);

            if (data != null && data.ContainsKey("result") && data["result"].ToString() == "failure")
            {
                throw new Exception(data["error"].ToString());
            }
            return response.Data;
        }


        private T Execute<T>(RestRequest request) where T : new()
        {
            var client = new RestClient(Const.WEB_API_ROOT);
            /*
                        client.Authenticator = new HttpBasicAuthenticator(_accountSid, _secretKey);
                        request.AddParameter("AccountSid", _accountSid, ParameterType.UrlSegment); // used on every request
            */
            var response = client.Execute<T>(request);

            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            return response.Data;
        }

        public async Task<List<RemoteFileInfo>> GetRemoteFileListAsync()
        {
            var request = CreateRequest("filelist", new Dictionary<string, string>());
            return await ExecuteAsync<List<RemoteFileInfo>>(request);
        }

        public byte[] DownloadFile(string filePath)
        {
            var client = new RestClient(Const.WEB_API_ROOT);
            return client.DownloadData(new RestRequest("filedata?path=" + filePath));
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