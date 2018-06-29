using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DotNetHelper;
using Heroes.ReplayParser;
using HotsBpHelper.Api;
using HotsBpHelper.Api.Model;
using HotsBpHelper.Api.Security;
using Newtonsoft.Json;

namespace HotsBpHelper.Uploader
{
    public class BpHelperUploader : Uploader
    {
        private readonly IRestApi _restApi;
        
        public BpHelperUploader(IRestApi restApi)
        {
            _restApi = restApi;
        }

        /// <summary>
        /// Upload replay
        /// </summary>
        /// <param name="file"></param>
        public override async Task Upload(ReplayFile file)
        {
            var result = DataParser.ParseReplayFull(file.Filename, false, false, false);
            FilePath tempPath = Path.GetTempFileName();
            tempPath = tempPath.GetDirPath() + tempPath.GetFileNameWithoutExtension() + ".json";
            try
            {
                var resultText = JsonConvert.SerializeObject(result.Item2);
                File.WriteAllText(tempPath, resultText);
            }
            catch (Exception)
            {
                file.HotsWeekUploadStatus = UploadStatus.UploadError;
                return;
            }

            file.HotsWeekUploadStatus = await Upload(tempPath, file.Fingerprint);
        }

        /// <summary>
        /// Upload replay
        /// </summary>
        /// <param name="file">Path to file</param>
        /// <param name="fingerprint"></param>
        /// <returns>Upload result</returns>
        private async Task<UploadStatus> Upload(string file, string fingerprint)
        {
            try
            {
                return await _restApi.UploadReplay(file, fingerprint);
            }
            catch (WebException ex)
            {
                if (await CheckApiThrottling(ex.Response))
                {
                    return await Upload(file, fingerprint);
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
                if (file.Created < Const.HotsWeekAcceptTime)
                {
                    file.HotsWeekUploadStatus = UploadStatus.TooOld;
                    continue;
                }

                fileIds.Add(new ReplayIdentity()
                {
                    FingerPrint = file.Fingerprint,
                    Md5 = CalculateMD5(file.Filename),
                    Size = new FileInfo(file.Filename).Length
                });

            }

            if (!fileIds.Any())
                return;

            var checkStatus = await CheckDuplicate(fileIds);
            foreach (var fingerPrintInfo in checkStatus.Status)
            {
                var file = replays.FirstOrDefault(f => f.Fingerprint == fingerPrintInfo.FingerPrint);
                if (file == null)
                    continue;

                if (fingerPrintInfo.Access == FingerPrintStatus.Reserved)
                    file.HotsWeekUploadStatus = UploadStatus.Reserved;

                if (fingerPrintInfo.Access == FingerPrintStatus.Duplicated)
                    file.HotsWeekUploadStatus = UploadStatus.Duplicate;

            }
            //  replays.Where(x => exists.Contains(x.Fingerprint)).Map(x => x.UploadStatus = UploadStatus.Duplicate);
        }

        /// <summary>
        /// Get minimum HotS client build supported by HotsApi
        /// </summary>
        public async Task<int> GetMinimumBuild()
        {
            try
            {
                int build = await _restApi.GetMinimalBuild();;
                return build;
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
