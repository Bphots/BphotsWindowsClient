using System.Collections.Generic;
using System.Windows;
using NLog;
using Stylet;
using StyletIoC;
using WPFLocalizeExtension.Extensions;

namespace HotsBpHelper.Pages
{
    public class ViewModelBase : Screen
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        protected IWindowManager WindowManager { get; set; }

        protected static string L(string key)
        {
            return LocExtension.GetLocalizedValue<string>(key);
        }

        protected MessageBoxResult ShowMessageBox(string messageBoxText,
            MessageBoxButton buttons = MessageBoxButton.OK,
            MessageBoxImage icon = MessageBoxImage.None,
            MessageBoxResult defaultResult = MessageBoxResult.None,
            MessageBoxResult cancelResult = MessageBoxResult.None,
            FlowDirection? flowDirection = null,
            TextAlignment? textAlignment = null)
        {
            var buttonsLabels = new Dictionary<MessageBoxResult, string>
            {
                {MessageBoxResult.Cancel, L("Cancel")},
                {MessageBoxResult.No, L("No")},
                {MessageBoxResult.None, L("None")},
                {MessageBoxResult.OK, L("OK")},
                {MessageBoxResult.Yes, L("Yes")}
            };

            return WindowManager.ShowMessageBox(messageBoxText, L("HotsBpHelper"), buttons, icon, defaultResult, cancelResult, buttonsLabels, flowDirection, textAlignment);
        }
    }
}