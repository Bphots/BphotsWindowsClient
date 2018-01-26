using System;
using System.Windows;
using DotNetHelper.Properties;
using HotsBpHelper.Pages;
using Stylet;
using StyletIoC;
using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace HotsBpHelper.Services
{
    public interface IToastService
    {
        void ShowInformation(string message);

        void ShowSuccess(string message);

        void ShowError(string message);

        void ShowWarning(string message);

        void CloseMessages(string message);
    }

    [UsedImplicitly]
    public class ToastService : IToastService, IDisposable
    {
        private readonly Notifier _notificationManager;

        private readonly MessageOptions _toastOptions = new MessageOptions
        {
            ShowCloseButton = false,
            FreezeOnMouseEnter = true,
            UnfreezeOnMouseLeave = false,
            NotificationClickAction = n => { n.Close(); }
        };

        public ToastService()
        {
            _notificationManager = new Notifier(cfg =>
            {
                cfg.PositionProvider = new PrimaryScreenPositionProvider(Corner.BottomRight, 5, 65);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(TimeSpan.FromSeconds(5),
                    MaximumNotificationCount.FromCount(3));

                cfg.Dispatcher = Application.Current.Dispatcher;
                cfg.DisplayOptions.TopMost = true;
                cfg.DisplayOptions.Width = 250;
            });
        }

        public void Dispose()
        {
            _notificationManager.Dispose();
        }

        public void ShowInformation(string message)
        {
            Execute.OnUIThread(() =>
            _notificationManager.ShowInformation(message, _toastOptions));
        }

        public void ShowSuccess(string message)
        {
            Execute.OnUIThread(() =>
               _notificationManager.ShowSuccess(message, _toastOptions));
        }

        public void ShowError(string message)
        {
            Execute.OnUIThread(() =>
               _notificationManager.ShowError(message, _toastOptions));
        }

        public void ShowWarning(string message)
        {
            Execute.OnUIThread(() =>
               _notificationManager.ShowWarning(message, _toastOptions));
        }

        public void CloseMessages(string message)
        {
            Execute.OnUIThread(() =>
               _notificationManager.ClearMessages(message));
        }
    }

}