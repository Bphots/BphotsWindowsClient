﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using HotsBpHelper.Api.Model;
using HotsBpHelper.Models;
using HotsBpHelper.Settings;
using ImageProcessor.Ocr;
using NLog;

namespace HotsBpHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string AppPath;
        public static Position MyPosition;

        public static AppSetting AppSetting;

        public static CustomConfigurationSettings CustomConfigurationSettings;

        public static CustomConfigurationSettings NextConfigurationSettings;

        public static UserDataSettings UserDataSettings { get; set; } = new UserDataSettings { LastClientVisit = DateTime.Now };

        public static List<HeroInfo> OcrHeroInfos;

        public static List<MapInfo> OcrMapInfos;

        public static bool Debug;

        public static bool ForceUpdate;

        public static DateTime UploadMinimumAcceptableTime;

        public static string Language = CultureInfo.CurrentCulture.Name;

        public static OcrLanguage OcrLanguage;

        public static About About = new About();

        public static bool DevTool { get; set; }

        public static int HotsWeekDelay { get; set; } = 100;

        public static Dictionary<int, HeroInfoV2> AdviceHeroInfos { get; set; }

        public static Dictionary<string, MapInfoV2> AdviceMapInfos { get; set; }

        public static bool HasServiceAsked { get; set; }

        public static bool HasHotsweekAsked { get; set; }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Pages.ErrorView _errorView;
            try
            {
                if (e.Exception.Message.ToLower().Contains("lang"))
                {
                    _errorView = new Pages.ErrorView(e.Exception.Message + "\nApplication language=" + App.Language);
                }
                else _errorView = new Pages.ErrorView(e.Exception.Message);

                _log.Error(e.Exception);
                _errorView.ShowDialog();
                _errorView.Pause();
                //MessageBox.Show(e.Exception.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            }
            catch (Exception)
            {
                // ignored
            }
            //Current.Shutdown();
        }

        private static Logger _log = LogManager.GetCurrentClassLogger();
    }
}
