using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Heroes.ReplayParser;
using NLog;

namespace HotsBpHelper.Uploader
{
    public class Analyzer
    {
        public int MinimumBuild { get; set; }
        
        private static Logger _log = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Analyze replay locally before uploading
        /// </summary>
        /// <param name="file">Replay file</param>
        public bool Analyze(ReplayFile file)
        {
            try
            {
                if (!string.IsNullOrEmpty(file.Fingerprint))
                    return true;

                var result = DataParser.ParseReplay(file.Filename, false, false, false);
                var replay = result.Item2;
                var parseResult = result.Item1;
                var status = GetPreStatus(file, replay, parseResult);
                
                if (status != null)
                {
                    file.HotsweekUploadStatus = file.HotsApiUploadStatus = status.Value;
                }

                if (parseResult != DataParser.ReplayParseResult.Success) {
                    return false;
                }

                file.Fingerprint = GetFingerprint(replay);
                file.GameMode = replay.GameMode;
                
                return true;
            }
            catch (Exception e) {
                _log.Warn(e, $"Error analyzing file {file}");
                return false;
            }
        }

        public UploadStatus? GetPreStatus(ReplayFile file, Replay replay, DataParser.ReplayParseResult parseResult)
        {
            switch (parseResult) {
                case DataParser.ReplayParseResult.ComputerPlayerFound:
                case DataParser.ReplayParseResult.TryMeMode:
                    return UploadStatus.AiDetected;
                case DataParser.ReplayParseResult.PTRRegion:
                    return UploadStatus.PtrRegion;
                case DataParser.ReplayParseResult.Incomplete:
                    return UploadStatus.Incomplete;
                case DataParser.ReplayParseResult.Exception:
                    if ((DateTime.Now - file.Created).Days > 7)
                        return UploadStatus.Incomplete;
                    return UploadStatus.UploadError;
                case DataParser.ReplayParseResult.PreAlphaWipe:
                    return UploadStatus.TooOld;
            }

            if (parseResult != DataParser.ReplayParseResult.Success) {
                return null;
            }

            if (replay.GameMode == GameMode.Custom) {
                return UploadStatus.CustomGame;
            }

            if (replay.ReplayBuild < MinimumBuild) {
                return UploadStatus.TooOld;
            }

            return null;
        }

        /// <summary>
        /// Get unique hash of replay. Compatible with HotsLogs
        /// </summary>
        /// <param name="replay"></param>
        /// <returns></returns>
        public string GetFingerprint(Replay replay)
        {
            var str = new StringBuilder();
            replay.Players.Select(p => p.BattleNetId).OrderBy(x => x).Map(x => str.Append(x.ToString()));
            str.Append(replay.RandomValue);
            var md5 = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(str.ToString()));
            var result = new Guid(md5);
            return result.ToString();
        }

        /// <summary>
        /// Swaps two bytes in a byte array
        /// </summary>
        private void SwapBytes(byte[] buf, int i, int j)
        {
            byte temp = buf[i];
            buf[i] = buf[j];
            buf[j] = temp;
        }
    }
}
