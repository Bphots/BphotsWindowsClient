using System;
using System.Collections.Generic;
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
using Stylet;

namespace HotsBpHelper.Utils
{
    public class OcrUtil
    {
        private readonly Recognizer _recognizer;
        
        public static bool NotInFocus = false;

        public static bool InGame = false;

        public static bool SuspendScanning => InGame || NotInFocus;

        public OcrUtil()
        {
            _recognizer = new Recognizer(App.OcrLanguage, Path.Combine(App.AppPath, @"Images\Heroes\"));
        }

        public void Dispose()
        {
            _recognizer.Dispose();
        }

        public async Task LookForBpScreen(CancellationToken cancellationToken)
        {
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

        public async Task<string> LookForMap(CancellationToken cancellationToken)
        {
            var finder = new Finder();
            var sb = new StringBuilder();
            var logUtil = new LogUtil(@".\logLookForMap.txt");
            logUtil.Log("Started");

            int attempts = 0;
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
                    _recognizer.ProcessMap(finder.CaptureMapArea(screenPath), sb);
                    if (!string.IsNullOrEmpty(sb.ToString()))
                        break;
                    attempts++;
                }
                if (attempts == 5)
                {
                    return string.Empty;
                }

                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
            }
            logUtil.Flush();
            return sb.ToString();
        }

        public async Task<List<string>> LookForLoadingLabels()
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
            List<int> ids = side == ScanSide.Right ? new List<int>() {9, 10} : new List<int> {2, 3};
            var sb = new StringBuilder();
            Finder finder = new Finder();
            var fileDic = new Dictionary<int, string>();
            var logUtil = new LogUtil(@".\logTeam" + string.Join("&", ids) + ".txt");
            int attempt = 0;
            while (attempt < 5)
            {
                lock (ImageProcessingHelper.GDILock)
                {
                    finder.AddNewTemplate(ids[0], ids[0].ToString(), fileDic);
                    logUtil.Log("Capture Complete " + ids[0]);
                    _recognizer.Recognize(fileDic[ids[0]], rotation, sb, 5);
                }
                if (sb.ToString() == _recognizer.PickingText)
                    break;

                attempt++;
                await Task.Delay(500);
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
                finder.AddNewTemplate(ids[1], ids[1].ToString(), fileDic);
                logUtil.Log("Second Capture Complete " + ids[0]);
                _recognizer.Recognize(fileDic[ids[1]], rotation, sb, 5);
            }
            logUtil.Log(sb.ToString());
            logUtil.Flush();
            return sb.ToString() == _recognizer.PickingText;
        }

        public async Task ScanLabelAsync(List<int> ids, BpViewModel bpViewModel, ScanSide side, CancellationToken cancellationToken)
        {
            try
            {
                var finder = new Finder();
                if (ids.Count == 1 && (ids[0] == 2 || ids[0] == 7))
                {
                    var stageInfo = new StageInfo();
                    while (stageInfo.Step < 2 && !cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(500);
                        stageInfo = finder.GetStageInfo();
                    }
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        bool isInTeamMatch =
                            await CheckIfInTeamMatchAsync(side, cancellationToken).ConfigureAwait(false);
                        if (isInTeamMatch)
                        {
                            bpViewModel.CancelScan();
                            return;
                        }
                    }
                }

                float rotation = side == ScanSide.Left ? (float)29.7 : (float)-29.7;

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

                var fileDic = new Dictionary<int, string>();

                var logUtil = new LogUtil(@".\logPick" + string.Join("&", ids) + ".txt");
                logUtil.Log("PickChecked");

                int bpScreenFail = 0;
                bool warned = false;
                while (checkedDic.Any(c => !c.Value))
                {
                    var sb = new StringBuilder();
                    var sbConfirm = new StringBuilder();
                    bool awaitFlag = false;
                    for (var i = 0; i < ids.Count; ++i)
                    {
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
                        var alreadyTrustable = false;

                        // first attempt
                        lock (ImageProcessingHelper.GDILock)
                        {
                            using (var bitmap = new ImageUtils().CaptureScreen())
                            {
                                if (!warned && bpViewModel.BpStatus.CurrentStep < 9 &&
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

                                logUtil.Log("Starting detect overlap");
                                if (side == ScanSide.Right)
                                {
                                    logUtil.Log("Starting right");
                                    var chartInfo = FrameFinder.CheckIfInChat(bitmap);
                                    logUtil.Log("Detect chat mode end");
                                    if (chartInfo == FrameFinder.ChatInfo.Partial && ids[i] > 10 ||
                                        chartInfo == FrameFinder.ChatInfo.Full)
                                    {
                                        logUtil.Log("Overlap detected");
                                        awaitFlag = true;
                                        continue;
                                    }

                                    var hasFrame = FrameFinder.CheckIfInRightFrame(bitmap);
                                    logUtil.Log("Detect right frame end");
                                    if (hasFrame)
                                    {
                                        logUtil.Log("Overlap detected");
                                        awaitFlag = true;
                                        continue;
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
                                        awaitFlag = true;
                                        continue;
                                    }
                                }

                                sb.Clear();
                                sbConfirm.Clear();

                                finder.AddNewTemplate(ids[i], ids[i].ToString(), fileDic, bitmap);
                                logUtil.Log("Capture Complete " + ids[i]);
                                alreadyTrustable = _recognizer.Recognize(fileDic[ids[i]], rotation, sb,
                                    App.AppSetting.Position.Height > 1200 ? 5 : 3);
                            }
                        }


                        logUtil.Log("Checked " + ids[i]);

                        if (sb.ToString() == _recognizer.PickingText || string.IsNullOrEmpty(sb.ToString()))
                            continue;

                        if (cancellationToken.IsCancellationRequested)
                            return;

                        if (SuspendScanning)
                        {
                            await Task.Delay(1000).ConfigureAwait(false);
                            continue;
                        }

                        // second attempt
                        FilePath tempscreenshotPath = null; 
                        if (!alreadyTrustable)
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
                                finder.AddNewTemplate(ids[i], ids[i].ToString(), fileDic, tempscreenshotPath);
                                logUtil.Log("Capture Complete " + ids[i]);
                                _recognizer.Recognize(fileDic[ids[i]], rotation, sbConfirm, App.AppSetting.Position.Height > 1200 ? 5 : 3);
                            } 
                        }

                        if (SuspendScanning)
                        {
                            await Task.Delay(1000).ConfigureAwait(false);
                            continue;
                        }

                        if (bpVm[ids[i]].Selected)
                        {
                            checkedDic[i] = true;
                            continue;
                        }

                        if (alreadyTrustable)
                            bpScreenFail = 0;

                        if ((alreadyTrustable || sb.ToString() == sbConfirm.ToString()) &&
                            !cancellationToken.IsCancellationRequested)
                        {
                            tempscreenshotPath?.DeleteIfExists();
                            var text = sb.ToString();
                            var index = ids[i];
                            Execute.OnUIThread(() => { bpViewModel.ShowHeroSelector(index, text); });
                            checkedDic[i] = true;
                            logUtil.Log("Confirmed " + index + " " + text);
                        }
                    }

                }
                logUtil.Flush();
            }
            catch (Exception e)
            {
                File.WriteAllText(@".\error" + string.Join("&", ids) + ".txt", e.Message + e.StackTrace + e);
                throw;
            }
        }
        
        
    }
}