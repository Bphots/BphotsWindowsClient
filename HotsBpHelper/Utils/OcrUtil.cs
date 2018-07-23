using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DotNetHelper;
using HotsBpHelper.HeroFinder;
using HotsBpHelper.Pages;
using ImageProcessor;
using ImageProcessor.ImageProcessing;
using ImageProcessor.Ocr;
using NLog;
using Stylet;

namespace HotsBpHelper.Utils
{
    public class OcrUtil
    {
        private Recognizer _recognizer;
        
        public static bool NotInFocus = false;

        public static bool InGame = false;
        private readonly int _trustableThreashold = App.AppSetting.Position.Height > 1200 ? 5 : 3;

        private static readonly List<int> LeftIdList = new List<int> { 3, 4, 5, 6, 7 };

        private static readonly List<int> RightIdList = new List<int> { 11, 12, 13, 14, 15 };

        private static readonly Dictionary<ScanSide, List<int>> IdList = new Dictionary<ScanSide, List<int>>
        {
            {ScanSide.Left, LeftIdList},
            {ScanSide.Right, RightIdList}
        };

        public static bool SuspendScanning => InGame || NotInFocus;

        private ConcurrentDictionary<int, bool> ProcessedPositions { get; } = new ConcurrentDictionary<int, bool>();

        public OcrUtil()
        {
        }

        public void ClearProcessedPositions()
        {
            ProcessedPositions.Clear();
        }

        public bool IsInitialized { get; set; }

        public void Dispose()
        {
            if (IsInitialized)
                _recognizer.Dispose();

            IsInitialized = false;
        }

        public void Initialize()
        {
            if (!IsInitialized)
                _recognizer = new Recognizer(App.OcrLanguage, Path.Combine(App.AppPath, @"Images\Heroes\"));

            IsInitialized = true;
        }

        public async Task LookForBpScreen(CancellationToken cancellationToken)
        {
            try
            {
                OcrAsyncChecker.CheckThread(OcrAsyncChecker.LookForBpScreenAsyncChecker);

                var finder = new Finder();
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (SuspendScanning)
                    {
                        await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    lock (ImageProcessingHelper.GDILock)
                    {
                        var screenPath = finder.CaptureScreen();
                        var isBp = ImageProcessingHelper.CheckIfInBp(screenPath);
                        if (isBp)
                            return;
                    }

                    await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                OcrAsyncChecker.CleanThread(OcrAsyncChecker.LookForBpScreenAsyncChecker);
            }
        }

        public async Task<string> LookForMap(CancellationToken cancellationToken)
        {
            try
            {
                OcrAsyncChecker.CheckThread(OcrAsyncChecker.LookForMapAsyncChecker);

                var finder = new Finder();
                var sb = new StringBuilder();
                var logUtil = new LogUtil(@".\logLookForMap.txt");
                logUtil.Log("Started");

                int attempts = 0;
                while (!cancellationToken.IsCancellationRequested && IsInitialized)
                {
                    sb.Clear();
                    if (SuspendScanning)
                    {
                        await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                        continue;
                    }
                    bool fullyTrustable = false;
                    lock (ImageProcessingHelper.GDILock)
                    {
                        var screenPath = finder.CaptureScreen();
                        fullyTrustable = _recognizer.ProcessMap(finder.CaptureMapArea(screenPath), sb);
                    }

                    if (!string.IsNullOrEmpty(sb.ToString()))
                    {
                        if (fullyTrustable)
                            break;

                        await Task.Delay(500, cancellationToken).ConfigureAwait(false);
                        var sbConfirm = new StringBuilder();
                        lock (ImageProcessingHelper.GDILock)
                        {
                            var screenPath = finder.CaptureScreen();
                            _recognizer.ProcessMap(finder.CaptureMapArea(screenPath), sbConfirm);
                            if (sbConfirm.ToString() == sb.ToString())
                                break;
                        }
                    }

                    attempts++;
                    if (attempts == 5)
                    {
                        return string.Empty;
                    }

                    await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                }
                logUtil.Flush();
                return sb.ToString();
            }
            finally
            {
                OcrAsyncChecker.CleanThread(OcrAsyncChecker.LookForMapAsyncChecker);
            }
        }

        public List<string> LookForLoadingLabels()
        {
            var finder = new Finder();
            var sb = new StringBuilder();
            var heroes = new List<string>();
            var logUtil = new LogUtil(@".\logLookForLoadingLabels.txt");
            logUtil.Log("Started");
            
            lock (ImageProcessingHelper.GDILock)
            {
                using (var bitMap = (new ImageUtils()).CaptureScreen())
                {
                    for (int i = 0; i < 5; ++i)
                    {
                        sb.Clear();
                        _recognizer.ProcessLoadingHero(finder.CaptureLeftLoadingLabel(bitMap, i), sb);
                        heroes.Add(sb.ToString());
                    }

                    for (int i = 0; i < 5; ++i)
                    {
                        sb.Clear();
                        _recognizer.ProcessLoadingHero(finder.CaptureRightLoadingLabel(bitMap, i), sb);
                        heroes.Add(sb.ToString());
                    }
                }
            }

            logUtil.Flush();
            return heroes;
        }

        public void AdjustPlaceHolderPosition()
        {
            var finder = new Finder();
            FilePath screenPath = finder.CaptureScreen();
            var points = ImageProcessingHelper.LookForPoints(screenPath);
            var height = ScreenUtil.GetScreenResolution().Height;
            App.AppSetting.Position.Left.HeroName1 = new Point(points[0] + (int) (0.0017*height),
                points[1] + (int) (0.0035*ScreenUtil.GetScreenResolution().Height));
            App.AppSetting.Position.Right.HeroName1 = new Point(points[2] + (int) (0.0035*height),
                points[3] + (int) (0.0045*ScreenUtil.GetScreenResolution().Height));
            File.WriteAllText(@".\coord.txt",
                App.AppSetting.Position.Left.HeroName1.X + @" " + App.AppSetting.Position.Left.HeroName1.Y + " " +
                App.AppSetting.Position.Right.HeroName1.X + " " + App.AppSetting.Position.Right.HeroName1.Y);
        }

      
        public enum ScanSide
        {
            Left,
            Right
        }

        public async Task<bool> CheckIfInTeamMatchAsync(ScanSide side, CancellationToken cancellationToken)
        {
            float rotation = side == ScanSide.Left ? (float) 29.7 : (float) -29.7;
            List<int> ids = side == ScanSide.Right ? new List<int>() {11, 12} : new List<int> {4, 5};
            var sb = new StringBuilder();
            Finder finder = new Finder();
            var fileDic = new Dictionary<int, string>();
            var logUtil = new LogUtil(@".\logTeam" + string.Join("&", ids) + ".txt");
            int attempt = 0;
            while (attempt < 5)
            {
                lock (ImageProcessingHelper.GDILock)
                {
                    using (var bitmap = finder.AddNewTemplate(ids[1], true))
                    {
                        logUtil.Log("Capture Complete " + ids[1]);
                        _recognizer.Recognize(bitmap, rotation, sb, 5, true);
                    }
                }
                if (sb.ToString() == _recognizer.PickingText)
                    break;

                attempt++;
                await Task.Delay(500, cancellationToken);
            }
            if (attempt == 5)
            {
                logUtil.Log("Giving up team match capturing " + ids[0]);
                logUtil.Flush();
                return false;
            }

            sb.Clear();
            lock (ImageProcessingHelper.GDILock)
            {
                using (var bitmap = finder.AddNewTemplate(ids[1], true))
                {
                    logUtil.Log("Second Capture Complete " + ids[1]);
                    _recognizer.Recognize(bitmap, rotation, sb, 5, true);
                }
            }
            logUtil.Log(sb.ToString());
            logUtil.Flush();
            return sb.ToString() == _recognizer.PickingText;
        }

        public async Task ScanLabelAsync(List<int> ids, BpViewModel bpViewModel, ScanSide side, CancellationToken cancellationToken)
        {
            try
            {
                OcrAsyncChecker.CheckThread(OcrAsyncChecker.ScanLabelAsyncChecker);

                var finder = new Finder();

                float rotation = side == ScanSide.Left ? (float) 29.7 : (float) -29.7;

                var bpVm = new Dictionary<int, HeroSelectorViewModel>();
                var checkedDic = new Dictionary<int, bool>();
                foreach (var id in ids)
                {
                    var vm = bpViewModel.HeroSelectorViewModels.First(v => v.Id == id);
                    if (vm == null)
                        return;

                    bpVm[id] = vm;
                }

                for (var i = 0; i < ids.Count; ++i)
                {
                    checkedDic[i] = false;
                }
                
                var logUtil = new LogUtil(@".\logPick" + string.Join("&", ids) + ".txt");
                logUtil.Log("PickChecked");

                int bpScreenFail = 0;
                bool warned = false;
                bool awaitFlag = false;
                while (checkedDic.Any(c => !c.Value))
                {
                    var sb = new StringBuilder();
                    var sbConfirm = new StringBuilder();
                    var i = checkedDic.FirstOrDefault(l => !l.Value).Key;

                    if (awaitFlag)
                    {
                        await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                        awaitFlag = false;
                    }

                    if (checkedDic.ContainsKey(i) && checkedDic[i])
                        continue;

                    if (bpVm[ids[i]].Selected)
                    {
                        checkedDic[i] = true;
                        continue;
                    }

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    if (SuspendScanning)
                    {
                        await Task.Delay(1000).ConfigureAwait(false);
                        continue;
                    }

                    logUtil.Log("Starting detect quit");
                    var processedResult = ProcessedResult.Fail;

                    int realPositionId = ids[i];

                    // first attempt
                    lock (ImageProcessingHelper.GDILock)
                    {
                        using (var bitmap = new ImageUtils().CaptureScreen())
                        {
                            if (!warned && bpViewModel.BpStatus.CurrentStep < 11 &&
                                StageFinder.ProcessStageInfo(bitmap).Step == -1)
                            {
                                bpScreenFail++;
                                if (bpScreenFail == 5)
                                {
                                    warned = true;
                                    bpViewModel.WarnNotInBp();
                                }
                            }
                            else
                            {
                                bpScreenFail = 0;
                            }

                            if (CheckIsOverlap(side, logUtil, bitmap))
                            {
                                awaitFlag = true;
                                continue;
                            }

                            sb.Clear();
                            sbConfirm.Clear();

                            foreach (var scanSideId in IdList[side].Where(id => !ProcessedPositions.ContainsKey(id)))
                            {
                                using (var bitMap = finder.AddNewTemplateBitmap(scanSideId, bitmap))
                                {
                                    logUtil.Log("Capture Complete " + scanSideId);
                                    processedResult = _recognizer.Recognize(bitMap, rotation, sb,
                                        _trustableThreashold);
                                    if (processedResult != ProcessedResult.Fail)
                                    {
                                        realPositionId = scanSideId;
                                        break;
                                    }
                                }
                            }
                               
                        }
                    }
                        
                    logUtil.Log("Checked " + ids[i] + " (" + sb + ")");

                    if (sb.ToString() == _recognizer.PickingText || string.IsNullOrEmpty(sb.ToString()) || processedResult == ProcessedResult.Fail)
                        continue;

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    if (SuspendScanning)
                    {
                        logUtil.Log("SuspendScanning delay 1000 " + ids[i]);
                        await Task.Delay(1000).ConfigureAwait(false);
                        continue;
                    }

                    // second attempt
                    FilePath tempscreenshotPath = null;
                    if (processedResult != ProcessedResult.Trustable)
                    {
                        logUtil.Log("Delay 500 " + ids[i]);
                        await Task.Delay(500).ConfigureAwait(false);
                        if (cancellationToken.IsCancellationRequested)
                            return;
                        if (SuspendScanning)
                        {
                            await Task.Delay(1000).ConfigureAwait(false);
                            continue;
                        }

                        lock (ImageProcessingHelper.GDILock)
                        {
                            tempscreenshotPath = finder.CaptureScreen();
                            using (var bitMap = finder.AddNewTemplateBitmap(realPositionId, tempscreenshotPath))
                            {
                                logUtil.Log("Capture Complete " + realPositionId);
                                _recognizer.Recognize(bitMap, rotation, sbConfirm,
                                    _trustableThreashold);
                            }
                        }
                    }

                    logUtil.Log("Second checked " + ids[i] + " (" + sbConfirm.ToString() + ")");

                    if (SuspendScanning)
                    {
                        logUtil.Log("SuspendScanning delay 1000 " + ids[i]);
                        await Task.Delay(1000).ConfigureAwait(false);
                        continue;
                    }

                    if (bpVm[ids[i]].Selected)
                    {
                        logUtil.Log("Vm already selected delay 1000 " + ids[i]);
                        checkedDic[i] = true;
                        continue;
                    }

                    if (processedResult == ProcessedResult.Trustable)
                        bpScreenFail = 0;

                    if ((processedResult == ProcessedResult.Trustable || sb.ToString() == sbConfirm.ToString()) &&
                        !cancellationToken.IsCancellationRequested && CheckIfInFocus())
                    {
                        bpScreenFail = 0;
                        tempscreenshotPath?.DeleteIfExists();
                        var text = sb.ToString();
                        var index = ids[checkedDic.First(l => !l.Value).Key];

                        if (ids.Contains(realPositionId) && !checkedDic[ids.IndexOf(realPositionId)])
                            index = realPositionId;

                        Execute.OnUIThread(() => { bpViewModel.ShowHeroSelector(index, text); });
                        ProcessedPositions[realPositionId] = true;
                        checkedDic[ids.IndexOf(index)] = true;
                        logUtil.Log("Confirmed " + index + " " + text);
                    }

                }
                logUtil.Flush();
            }
            catch (Exception e)
            {
                File.WriteAllText(@".\error" + string.Join("&", ids) + ".txt", e.Message + e.StackTrace + e);
                throw;
            }
            finally
            {
                OcrAsyncChecker.CleanThread(OcrAsyncChecker.ScanLabelAsyncChecker);
            }
        }

        private static bool CheckIsOverlap(ScanSide side, LogUtil logUtil, Bitmap bitmap)
        {
            logUtil.Log("Starting detect overlap");
            if (side == ScanSide.Right)
            {
                logUtil.Log("Starting right");
                var chartInfo = FrameFinder.CheckIfInChat(bitmap);
                logUtil.Log("Detect chat mode end");
                if (chartInfo == FrameFinder.ChatInfo.Partial ||
                    chartInfo == FrameFinder.ChatInfo.Full)
                {
                    logUtil.Log("Overlap detected");
                    return true;
                }

                var hasFrame = FrameFinder.CheckIfInRightFrame(bitmap);
                logUtil.Log("Detect right frame end");
                if (hasFrame)
                {
                    logUtil.Log("Overlap detected");
                    return true;
                }
            }
            else
            {
                logUtil.Log("Starting left");
                var hasFrame = FrameFinder.CheckIfInLeftFrame(bitmap);
                logUtil.Log("Detect left frame end");
                if (hasFrame)
                {
                    logUtil.Log("Overlap detected");
                    return true;
                }
            }
            return false;
        }

        private static bool CheckIfInFocus()
        {
            var hwnd = Win32.GetForegroundWindow();
            var pid = Win32.GetWindowProcessID(hwnd);
            var process = Process.GetProcessById(pid);
            var inHotsGame = process.ProcessName.StartsWith(Const.HEROES_PROCESS_NAME);
            var inHotsHelper = process.ProcessName.StartsWith(Const.HOTSBPHELPER_PROCESS_NAME);
            return inHotsGame || inHotsHelper;
        }

    }

    public static class OcrAsyncChecker
    {
        public const int LookForBpScreenAsyncChecker = 0;
        public const int LookForMapAsyncChecker = 1;
        public const int ScanLabelAsyncChecker = 2;
        public const int AwaitStagAsyncChecker = 3;

        private static readonly List<bool> OcrAsyncThreads = new List<bool> {false, false, false, false};

        public static void CheckThread(int threadId)
        {
            if (!App.Debug && LogUtil.NoLog)
                return;

            Logger.Trace("Enter thread : {0}", threadId);
            if (OcrAsyncThreads[threadId])
                Logger.Trace("Duplicate thread detected : {0}", threadId);

            for (int i = 0; i < OcrAsyncThreads.Count; ++i)
            {
                if (i != threadId && OcrAsyncThreads[i])
                Logger.Trace("Parallel thread detected with thread : {0} , {1}", i, threadId);
            }

            OcrAsyncThreads[threadId] = true;
        }

        public static void CleanThread(int threadId)
        {
            if (!App.Debug && LogUtil.NoLog)
                return;

            Logger.Trace("Exit thread: {0}", threadId);
            OcrAsyncThreads[threadId] = false;
        }
        
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    }
}