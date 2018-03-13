using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HotsBpHelper.Api;
using HotsBpHelper.Api.Model;
using HotsBpHelper.Api.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace HotsBpHelper.Uploader
{
    public abstract class Uploader
    {
        protected static Logger _log = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Upload replay
        /// </summary>
        /// <param name="file"></param>
        public abstract Task Upload(ReplayFile file);

        /// <summary>
        /// Upload replay
        /// </summary>
        /// <param name="file">Path to file</param>
        /// <returns>Upload result</returns>
        public abstract Task<UploadStatus> Upload(string file);

        /// <summary>
        /// Check if Hotsapi request limit is reached and wait if it is
        /// </summary>
        /// <param name="response">Server response to examine</param>
        protected async Task<bool> CheckApiThrottling(WebResponse response)
        {
            if (!(response is HttpWebResponse))
                return false;

            if ((int)((HttpWebResponse) response).StatusCode == 429)
            {
                _log.Warn($"Too many requests, waiting");
                await Task.Delay(10000);
                return true;
            }

            return false;
        }
    }
}
