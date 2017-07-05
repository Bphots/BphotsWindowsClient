using System;
using System.Windows;

namespace HotsBpHelper.Pages
{
    public partial class HeroSelectorView : Window
    {
        public HeroSelectorView()
        {
//            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            ComboBox.Focus();
        }
    }
}