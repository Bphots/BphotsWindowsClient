using System;
using System.Windows;
using System.Windows.Input;

namespace HotsBpHelper.Pages
{
    public partial class HeroSelectorView : Window
    {
        public HeroSelectorView()
        {
            //            InitializeComponent();
            Loaded += OnLoaded;
            KeyDown += OnKeyDown;
            PreviewKeyUp += OnPreviewKeyUp;
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
            var vm = (HeroSelectorViewModel)DataContext;
            if (keyEventArgs.Key == Key.Enter && vm.SelectedItemInfo != null)
            {
                vm.ConfirmSelection();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            ComboBox.Focus();
        }
    }
}