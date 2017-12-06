using System;
using System.Windows;
using System.Windows.Input;

namespace HotsBpHelper.Pages
{
    public partial class HeroSelectorView : Window
    {

        private bool isPressed = false;

        public HeroSelectorView()
        {
            //            InitializeComponent();
            Loaded += OnLoaded;
            KeyDown += OnKeyDown;
            PreviewKeyUp += OnPreviewKeyUp;

        }

        /*
                protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
                {
                    base.OnPreviewMouseLeftButtonUp(e);
                    isPressed = true;
                }

                protected override void OnMouseLeave(MouseEventArgs e)
                {
                    base.OnMouseLeave(e);
                    if (isPressed)
                    {
                        Confirm();
                        isPressed = false;
                    }
                }
        */

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape)
            {
                isPressed = false;
                Cancel();
            }
            isPressed = false;
        }

        public void Confirm()
        {

            var vm = (HeroSelectorViewModel)DataContext;
            if (vm.SelectedItemInfo != null)
            {
                vm.ConfirmSelection();
            }
        }

        public void Cancel()
        {
            var vm = (HeroSelectorViewModel)DataContext;
            vm.CancelSelection();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System && e.SystemKey == Key.F4)
            {
                e.Handled = true;
            }
        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key.Enter)
            {
                Confirm();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            ComboBox.Focus();
        }
    }
}