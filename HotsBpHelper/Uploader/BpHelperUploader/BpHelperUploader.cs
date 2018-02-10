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
                return await _restApi.UploadReplay(file);
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
