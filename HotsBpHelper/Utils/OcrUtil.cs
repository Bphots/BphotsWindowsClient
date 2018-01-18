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

        public static bool SuspendScanning = false;

        public OcrUtil()
        {
            try
            {
                var language = OcrLanguage.English;
                if (App.Language.Contains(@"CN"))
                    language = OcrLanguage.SimplifiedChinese;
                if (App.Language.Contains(@"TW"))
                    language = OcrLanguage.TraditionalChinese;
                
                _recognizer = new Recognizer(language, Path.Combine(App.AppPath, @"Images\Heroes\"));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.StackTrace);
                throw;
            }
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
                    var isBp = ImageProcessingHelper.LookForBpStats(screenPath);
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
        
        public void AdjustPlaceHolderPosition()
        {
            var finder = new Finder();
            FilePath screenPath = finder.CaptureScreen();
            var points = ImageProcessingHelper.LookForPoints(screenPath);
            var height = ScreenUtil.GetScreenResolution().Height;
            App.MyPosition.Left.HeroName1 = new Point(points[0] + (int) (0.0017*height),
                points[1] + (int) (0.0035*ScreenUtil.GetScreenResolution().Height));
            App.MyPosition.Right.HeroName1 = new Point(points[2] + (int) (0.0035*height),
                points[3] + (int) (0.0045*ScreenUtil.GetScreenResolution().Height));
            File.WriteAllText(@".\coord.txt",
                App.MyPosition.Left.HeroName1.X + @" " + App.MyPosition.Left.HeroName1.Y + " " +
                App.MyPosition.Right.HeroName1.X + " " + App.MyPosition.Right.HeroName1.Y);
        }

        public async Task<string> ScanLeftAsync(int i, CancellationToken cancellationToken)
        {
            try
            {
                var finder = new Finder();
                var fileDic = new Dictionary<int, string>();

                var logUtil = new LogUtil(@".\logLeftPick" + i + ".txt");
                logUtil.Log("PickChecked");

                var sb = new StringBuilder();
                var sbConfirm = new StringBuilder();
                while (sb.ToString() == _recognizer.PickingText || string.IsNullOrEmpty(sb.ToString()) ||
                       sbConfirm.ToString() != sb.ToString())
                {
                    if (cancellationToken.IsCancellationRequested)
                        return string.Empty;

                    if (sb.ToString() != _recognizer.PickingText)
                    {
                        logUtil.Log("Delay 1000");
                        await Task.Delay(1000).ConfigureAwait(false);
                    }
                    else
                    {
                        logUtil.Log("Delay 300");
                        await Task.Delay(300).ConfigureAwait(false);
                    }

                    sb.Clear();
                    sbConfirm.Clear();
                    Monitor.Enter(ImageProcessingHelper.GDILock);

                    try
                    {
                        finder.AddNewTemplate(i, i.ToString(), fileDic);
                        logUtil.Log("Capture Complete");
                        _recognizer.Recognize(fileDic[i], (float) 29.7, sb, i.ToString());
                    }
                    finally
                    {
                        Monitor.Exit(ImageProcessingHelper.GDILock);
                    }

                    logUtil.Log("Checked");
                    if (sb.ToString() == _recognizer.PickingText || string.IsNullOrEmpty(sb.ToString()))
                        continue;

                    logUtil.Log("Delay 500");
                    await Task.Delay(500).ConfigureAwait(false);
                    Monitor.Enter(ImageProcessingHelper.GDILock);
                    try
                    {
                        finder.AddNewTemplate(i, i.ToString(), fileDic);
                        logUtil.Log("Capture Complete");
                        _recognizer.Recognize(fileDic[i], (float) 29.7, sbConfirm, i.ToString());
                    }
                    finally
                    {
                        Monitor.Exit(ImageProcessingHelper.GDILock);
                    }
                    logUtil.Log("Confirmed");
                }
                logUtil.Flush();
                if (cancellationToken.IsCancellationRequested)
                    return string.Empty;

                return sb.ToString();
            }
            catch (Exception e)
            {
                File.WriteAllText(@".\error" + i + ".txt", e.Message + e.StackTrace + e);
                throw;
            }
        }

        public async Task ScanLeftAsync(List<int> ids, BpViewModel bpViewModel, CancellationToken cancellationToken)
        {
            try
            {
                var bpVm = new Dictionary<int, HeroSelectorViewModel>();
                var checkedDic = new Dictionary<int, bool>();
                foreach (var id in ids)
                {
                    bpVm[id] = bpViewModel.HeroSelectorViewModels.First(vm => vm.Id == id);
                    checkedDic[id] = false;
                }

                var finder = new Finder();
                var fileDic = new Dictionary<int, string>();

                var logUtil = new LogUtil(@".\logLeftPick" + string.Join("&", ids) + ".txt");
                logUtil.Log("PickChecked");
                while (checkedDic.Any(c => !c.Value))
                {
                    var sb = new StringBuilder();
                    var sbConfirm = new StringBuilder();
                    for (var i = 0; i < ids.Count; ++i)
                    {
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

                        sb.Clear();
                        sbConfirm.Clear();
                        var alreadyTrustable = false;

                        Monitor.Enter(ImageProcessingHelper.GDILock);
                        try
                        {
                            finder.AddNewTemplate(ids[i], ids[i].ToString(), fileDic);
                            logUtil.Log("Capture Complete " + ids[i]);
                            alreadyTrustable = _recognizer.Recognize(fileDic[ids[i]], (float) 29.7, sb, ids[i].ToString());
                        }
                        finally
                        {
                            Monitor.Exit(ImageProcessingHelper.GDILock);
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

                            Monitor.Enter(ImageProcessingHelper.GDILock);
                            try
                            {
                                finder.AddNewTemplate(ids[i], ids[i].ToString(), fileDic);
                                logUtil.Log("Capture Complete " + ids[i]);
                                _recognizer.Recognize(fileDic[ids[i]], (float) 29.7, sbConfirm, ids[i].ToString());
                            }
                            finally
                            {
                                Monitor.Exit(ImageProcessingHelper.GDILock);
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

                        if ((alreadyTrustable || sb.ToString() == sbConfirm.ToString()) &&
                            !cancellationToken.IsCancellationRequested)
                        {
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

        public async Task ScanRightAsync(List<int> ids, BpViewModel bpViewModel, CancellationToken cancellationToken)
        {
            try
            {
                var bpVm = new Dictionary<int, HeroSelectorViewModel>();
                var checkedDic = new Dictionary<int, bool>();
                foreach (var id in ids)
                {
                    bpVm[id] = bpViewModel.HeroSelectorViewModels.First(vm => vm.Id == id);
                    checkedDic[id] = false;
                }

                var finder = new Finder();
                var fileDic = new Dictionary<int, string>();

                var logUtil = new LogUtil(@".\logRightPick" + string.Join("&", ids) + ".txt");
                logUtil.Log("PickChecked");

                while (checkedDic.Any(c => !c.Value))
                {
                    var sb = new StringBuilder();
                    var sbConfirm = new StringBuilder();
                    for (var i = 0; i < ids.Count; ++i)
                    {
                        if (checkedDic.ContainsKey(i) && checkedDic[i])
                            continue;

                        if (cancellationToken.IsCancellationRequested)
                            return;

                        if (bpVm[ids[i]].Selected)
                        {
                            checkedDic[i] = true;
                            continue;
                        }

                        if (SuspendScanning)
                        {
                            await Task.Delay(1000).ConfigureAwait(false);
                            continue;
                        }

                        sb.Clear();
                        sbConfirm.Clear();
                        var alreadyTrustable = false;

                        Monitor.Enter(ImageProcessingHelper.GDILock);
                        try
                        {
                            finder.AddNewTemplate(ids[i], ids[i].ToString(), fileDic);
                            logUtil.Log("Capture Complete " + ids[i]);
                            alreadyTrustable = _recognizer.Recognize(fileDic[ids[i]], (float) -29.7, sb,
                                ids[i].ToString());
                        }
                        finally
                        {
                            Monitor.Exit(ImageProcessingHelper.GDILock);
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

                            Monitor.Enter(ImageProcessingHelper.GDILock);
                            try
                            {
                                finder.AddNewTemplate(ids[i], ids[i].ToString(), fileDic);
                                logUtil.Log("Capture Complete " + ids[i]);
                                _recognizer.Recognize(fileDic[ids[i]], (float) -29.7, sbConfirm, ids[i].ToString());
                            }
                            finally
                            {
                                Monitor.Exit(ImageProcessingHelper.GDILock);
                            }
                        }

                        if (bpVm[ids[i]].Selected)
                        {
                            checkedDic[i] = true;
                            continue;
                        }

                        if ((alreadyTrustable || sb.ToString() == sbConfirm.ToString()) &&
                            !cancellationToken.IsCancellationRequested)
                        {
                            if (SuspendScanning)
                            {
                                await Task.Delay(1000).ConfigureAwait(false);
                                continue;
                            }

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