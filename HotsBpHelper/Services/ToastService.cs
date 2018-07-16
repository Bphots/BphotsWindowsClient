using System;
using System.Windows;
using DotNetHelper.Properties;
using Stylet;
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

        void ShowInformation(string message, Action customAction);

        void DisposeManager();

        void ReinitializeToast();
    }

    [UsedImplicitly]
    public class ToastService : IToastService, IDisposable
    {
        private readonly MessageOptions _toastOptions = new MessageOptions
        {
            ShowCloseButton = false,
            FreezeOnMouseEnter = true,
            UnfreezeOnMouseLeave = false,
            NotificationClickAction = n => { n.Close(); }
        };

        private bool _isInitialzed;
        private Notifier _notificationManager;

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
            _isInitialzed = true;
        }

        public void Dispose()
        {
            if (!_isInitialzed)
                return;

            _notificationManager.Dispose();
        }

        public void ShowInformation(string message, Action customAction)
        {
            if (!_isInitialzed)
                return;

            var messageOption = new MessageOptions
            {
                ShowCloseButton = true,
                FreezeOnMouseEnter = true,
                UnfreezeOnMouseLeave = false,
                NotificationClickAction = n =>
                {
                    customAction.Invoke();
                    n.Close();
                }
            };

            Execute.OnUIThread(() =>
            {
                if (!_isInitialzed)
                    return;

                _notificationManager.ShowInformation(message, messageOption);
            });
        }

        public void DisposeManager()
        {
            if (!_isInitialzed)
                return;

            _isInitialzed = false;
            _notificationManager.ClearMessages();
            _notificationManager.Dispose();
        }

        public void ReinitializeToast()
        {
            if (_isInitialzed)
                return;

            _notificationManager = new Notifier(cfg =>
            {
                cfg.PositionProvider = new PrimaryScreenPositionProvider(Corner.BottomRight, 5, 65);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(TimeSpan.FromSeconds(5),
                    MaximumNotificationCount.FromCount(3));

                cfg.Dispatcher = Application.Current.Dispatcher;
                cfg.DisplayOptions.TopMost = true;
                cfg.DisplayOptions.Width = 250;
            });
            _isInitialzed = true;
        }

        public void ShowInformation(string message)
        {
            if (!_isInitialzed)
                return;

            Execute.OnUIThread(() =>
                _notificationManager.ShowInformation(message, _toastOptions));
        }

        public void ShowSuccess(string message)
        {
            if (!_isInitialzed)
                return;

            Execute.OnUIThread(() =>
                _notificationManager.ShowSuccess(message, _toastOptions));
        }

        public void ShowError(string message)
        {
            if (!_isInitialzed)
                return;

            Execute.OnUIThread(() =>
                _notificationManager.ShowError(message, _toastOptions));
        }

        public void ShowWarning(string message)
        {
            if (!_isInitialzed)
                return;

            Execute.OnUIThread(() =>
                _notificationManager.ShowWarning(message, _toastOptions));
        }

        public void CloseMessages(string message)
        {
            if (!_isInitialzed)
                return;

            Execute.OnUIThread(() =>
                _notificationManager.ClearMessages(message));
        }
    }
}