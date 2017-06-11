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

        public async Task<List<string>> GetHeroList(string language)
        {
            return await Task.FromResult(new List<string>()
            {
                "D.Va", "ETC", "阿巴瑟", "阿尔萨斯", "阿拉纳克", "阿努巴拉克", "阿塔尼斯", "阿兹莫丹", "奥利尔", "奔波尔霸", "查莉娅", "陈", "德哈卡", "迪亚波罗", "缝合怪", "弗斯塔德", "格雷迈恩", "古", "古尔丹", "光明之翼", "吉安娜", "加尔", "加兹鲁维", "卡拉辛姆", "卡西娅", "凯尔萨斯", "凯瑞甘", "克罗米", "拉格纳罗斯", "雷加尔", "雷克萨", "雷诺", "李奥瑞克", "李敏", "丽丽", "猎空", "卢西奥", "露娜拉", "玛法里奥", "麦迪文", "莫拉莉斯中尉", "穆拉丁", "纳兹波", "诺娃", "普罗比斯", "乔汉娜", "萨尔", "萨穆罗", "桑娅", "失落的维京人", "塔萨达尔", "泰凯斯", "泰兰德", "泰瑞尔", "屠夫", "瓦里安", "瓦莉拉", "维拉", "乌瑟尔", "希尔瓦娜斯", "伊利丹", "源氏", "泽拉图", "扎加拉", "重锤军士", "祖尔", "祖尔金"
            });
        }
    }
}