using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HotsBpHelper.UserControls;
using HotsBpHelper.WPF;

namespace HotsBpHelper.Pages
{
    public partial class HeroSelectorWindowView : TabSwitcherFreeWindow
    {
        private readonly List<FilteredComboBox> _filteredComboBoxes = new List<FilteredComboBox>();

        public HeroSelectorWindowView()
        {
            InitializeComponent();
        }

        private void Confirm(FilteredComboBox filteredComboBox)
        {
            var vm = (HeroSelectorViewModel) filteredComboBox.DataContext;
            if (vm.SelectedItemInfo != null)
            {
                vm.ConfirmSelection();
            }
        }

        private void Cancel(FilteredComboBox filteredComboBox)
        {
            var vm = (HeroSelectorViewModel) filteredComboBox.DataContext;
            vm.CancelSelection();
        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            var filteredComboBox = sender as FilteredComboBox;
            if (filteredComboBox == null)
                return;

            if (keyEventArgs.Key == Key.Enter)
            {
                Confirm(filteredComboBox);
                var index = _filteredComboBoxes.IndexOf(filteredComboBox);
                if (index == -1)
                    return;

                for (var i = index + 1; i < _filteredComboBoxes.Count; ++i)
                {
                    if (_filteredComboBoxes[i].SelectedIndex != -1) continue;

                    _filteredComboBoxes[i].Focus();
                    return;
                }
            }
        }

        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            var filteredComboBox = sender as FilteredComboBox;
            if (filteredComboBox == null)
                return;
            if (e.Key == Key.System && e.SystemKey == Key.F4)
            {
                e.Handled = true;
            }
            if (e.Key == Key.Escape)
            {
                filteredComboBox.IsPressed = false;
                Cancel(filteredComboBox);
            }
            filteredComboBox.IsPressed = false;
        }

        private void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var filteredComboBox = sender as FilteredComboBox;
            if (filteredComboBox == null)
                return;
            OnPreviewMouseLeftButtonUp(e);
            filteredComboBox.IsPressed = true;
        }

        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            var filteredComboBox = sender as FilteredComboBox;
            if (filteredComboBox == null)
                return;

            if (filteredComboBox.IsPressed)
            {
                Confirm(filteredComboBox);
                filteredComboBox.IsPressed = false;
            }
        }

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            var filteredComboBox = sender as FilteredComboBox;
            if (filteredComboBox == null)
                return;

            _filteredComboBoxes.Add(filteredComboBox);
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var filteredComboBox = sender as FilteredComboBox;
            if (filteredComboBox == null)
                return;
            
            if (!filteredComboBox.IsFocused)
                return;

            var index = _filteredComboBoxes.IndexOf(filteredComboBox);
            if (index == -1)
                return;

            for (var i = index + 1; i < _filteredComboBoxes.Count; ++i)
            {
                if (_filteredComboBoxes[i].SelectedIndex != -1) continue;

                _filteredComboBoxes[i].Focus();
                return;
            }
        }
    }
}