using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HotsBpHelper.Api;
using HotsBpHelper.Api.Model;
using HotsBpHelper.Api.Security;

namespace HotsBpHelper.Uploader
{
    public class BpHelperUploader : Uploader
    {
        private readonly ISecurityProvider _securityProvider;
        private readonly IRestApi _restApi;

        const string ApiEndpoint = "http://week.bphots.com:8888/replay/";

        public BpHelperUploader(ISecurityProvider securityProvider, IRestApi restApi)
        {
            _securityProvider = securityProvider;
            _restApi = restApi;
        }

        /// <summary>
        /// Upload replay
        /// </summary>
        /// <param name="file"></param>
        public override async Task Upload(ReplayFile file)
        {
            file.BpHelperUploadStatus = await Upload(file.Filename);
        }

        /// <summary>
        /// Upload replay
        /// </summary>
        /// <param name="file">Path to file</param>
        /// <returns>Upload result</returns>
        public override async Task<UploadStatus> Upload(string file)
        {
            try
            {
                var parameters = new List<Tuple<string, string>>();
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

                string response;
                using (var client = new WebClient())
                {
                    var bytes = await client.UploadFileTaskAsync($"{ApiEndpoint}upload?{urlParam}&nonce={sp.Nonce}&sign={sp.Sign}", file);
                    response = Encoding.UTF8.GetString(bytes);
                }

                return UploadStatus.Success;

                // TODO
                //dynamic json = JObject.Parse(response);
                //if ((bool)json.success) {
                //    if (Enum.TryParse<UploadStatus>((string)json.status, out UploadStatus status)) {
                //        _log.Debug($"Uploaded file '{file}': {status}");
                //        return status;
                //    } else {
                //        _log.Error($"Unknown upload status '{file}': {json.status}");
                //        return UploadStatus.UploadError;
                //    }
                //} else {
                //    _log.Warn($"Error uploading file '{file}': {response}");
                //    return UploadStatus.UploadError;
                //}
            }
            catch (WebException ex)
            {
                if (await CheckApiThrottling(ex.Response))
                {
                    return await Upload(file);
                }
                _log.Warn(ex, $"Error uploading file '{file}'");
                return UploadStatus.UploadError;
            }
        }


        /// <summary>
        /// Mass check replay fingerprints against database to detect duplicates
        /// </summary>
        public async Task<FingerPrintStatusCollection> CheckDuplicate(IEnumerable<ReplayIdentity> replayIdentities)
        {
            try
            {
                var response = await _restApi.CheckDuplicatesAsync(replayIdentities);
                return response;
            }
            catch (WebException ex)
            {
                if (await CheckApiThrottling(ex.Response))
                {
                    return await CheckDuplicate(replayIdentities);
                }
                _log.Warn(ex, $"Error checking fingerprint array");
                return null;
            }
        }

        private static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        /// <summary>
        /// Mass check replay fingerprints against database to detect duplicates
        /// </summary>
        public async Task CheckDuplicate(IList<ReplayFile> replays)
        {
            var fileIds = new List<ReplayIdentity>();
            foreach (var file in replays)
            {
                fileIds.Add(new ReplayIdentity()
                {
                    FingerPrint = file.Fingerprint,
                    Md5 = CalculateMD5(file.Filename),
                    Size = new FileInfo(file.Filename).Length
                });
            }

            var checkStatus = await CheckDuplicate(fileIds);
            foreach (var fingerPrintInfo in checkStatus.Status)
            {
                var file = replays.FirstOrDefault(f => f.Fingerprint == fingerPrintInfo.FingerPrint);
                if (file == null)
                    continue;

                if (fingerPrintInfo.Access == FingerPrintStatus.Reserved)
                    file.BpHelperUploadStatus = UploadStatus.Reserved;

                if (fingerPrintInfo.Access == FingerPrintStatus.Duplicated)
                    file.BpHelperUploadStatus = UploadStatus.Duplicate;

            }
            //  replays.Where(x => exists.Contains(x.Fingerprint)).Map(x => x.UploadStatus = UploadStatus.Duplicate);
        }

        /// <summary>
        /// Get minimum HotS client build supported by HotsApi
        /// </summary>
        public async Task<int> GetMinimumBuild()
        {
            var parameters = new List<Tuple<string, string>>();
            var sp = _securityProvider.CaculateSecurityParameter(parameters);

            parameters.Add(Tuple.Create("timestamp", sp.Timestamp));
            parameters.Add(Tuple.Create("client_patch", sp.Patch));

            string urlParam = string.Join("&", parameters.Select(tuple => $"{tuple.Item1}={tuple.Item2}"));
            try
            {
                using (var client = new WebClient())
                {
                    var response = await client.DownloadStringTaskAsync($"{ApiEndpoint}min-build?{urlParam}&nonce={sp.Nonce}&sign={sp.Sign}");
                    int build;
                    if (!int.TryParse(response, out build))
                    {
                        _log.Warn($"Error parsing minimum build: {response}");
                        return 0;
                    }
                    return build;
                }
            }
            catch (WebException ex)
            {
                if (await CheckApiThrottling(ex.Response))
                {
                    return await GetMinimumBuild();
                }
                _log.Warn(ex, $"Error getting minimum build");
                return 0;
            }
        }


    }
}
