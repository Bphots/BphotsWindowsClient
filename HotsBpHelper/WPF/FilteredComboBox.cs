using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using HotsBpHelper.Pages;

namespace HotsBpHelper.WPF
{
    public class FilteredComboBox : ComboBox
    {
        private string _currentFilter = string.Empty;

        public bool IsPressed;

        private string _oldFilter = string.Empty;

        protected TextBox EditableTextBox => GetTemplateChild("PART_EditableTextBox") as TextBox;

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
            //取消高光效果
        {
            Effect = null;
            if (e.AddedItems.Count == 0 && e.RemovedItems.Count > 0)
                Focus();
            base.OnSelectionChanged(e);
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            Effect = new DropShadowEffect() {Color = Color.FromArgb(24, 13, 47, 0), ShadowDepth = 0};
            base.OnGotKeyboardFocus(e);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);
            IsPressed = true;
        }

        protected override void OnDropDownOpened(EventArgs e)
        {
            if (IsPressed)
                ClearFilter();

            base.OnDropDownOpened(e);
        }
        protected override void OnDropDownClosed(EventArgs e)
        {
            ClearFilter();
            var temp = SelectedIndex;
            SelectedIndex = -1;
            Text = string.Empty;
            SelectedIndex = temp;
            base.OnDropDownClosed(e);
        }                     

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (newValue != null)
            {
                var view = CollectionViewSource.GetDefaultView(newValue);
                view.Filter += FilterItem;
            }

            if (oldValue != null)
            {
                var view = CollectionViewSource.GetDefaultView(oldValue);
                if (view != null) view.Filter -= FilterItem;
            }

            base.OnItemsSourceChanged(oldValue, newValue);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Tab:
                case Key.Enter:
                    if (Text != "" && SelectedIndex == -1) SelectedIndex = 0;
                    IsDropDownOpen = false;
                    break;
                case Key.Down:
                    IsDropDownOpen = true;
                    if (SelectedIndex == -1) SelectedIndex = 0;
                    base.OnPreviewKeyDown(e);
                    break;
                case Key.Up:
                    IsDropDownOpen = true;
                    base.OnPreviewKeyDown(e);
                    break;
                default:
                    base.OnPreviewKeyDown(e);
                    break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            IsPressed = false;
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                    base.OnKeyUp(e);
                    break;
                case Key.Tab:
                case Key.Enter:

                    ClearFilter();
                    base.OnKeyUp(e);
                    break;
                default:
                    base.OnKeyUp(e);
                    _currentFilter = Text;
                    if (_currentFilter != _oldFilter)
                    {
                        _oldFilter = Text;
                        IsDropDownOpen = true;
                        RefreshFilter(false);
                        _currentFilter = _oldFilter;
                        Text = _oldFilter;
                        EditableTextBox.SelectionStart = int.MaxValue;
                    }
                    break;
            }
        }

        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            ClearFilter();
            var temp = SelectedIndex;
            SelectedIndex = -1;
            Text = string.Empty;
            SelectedIndex = temp;
            base.OnPreviewLostKeyboardFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            var temp = SelectedIndex;
            SelectedIndex = -1;
            Text = string.Empty;
            SelectedIndex = temp;
            base.OnLostFocus(e);
        }

        private void RefreshFilter(bool reselect = true)
        {
            if (ItemsSource == null) return;

            var currentItem = SelectedItem;

            var view = CollectionViewSource.GetDefaultView(ItemsSource);
            view.Refresh();

            if (reselect && currentItem != null)
            {
                SelectedItem = Items.Cast<object>().FirstOrDefault(c => c == currentItem);
                if (SelectedItem != null)
                    SelectedIndex = Items.IndexOf(SelectedItem);
            }
        }

        private void ClearFilter()
        {
            _currentFilter = string.Empty;
            RefreshFilter();
        }

        private bool FilterItem(object value)
        {
            if (value == null) return false;
            if (_currentFilter.Length == 0) return true;
            var v = value.ToString().ToLower();
            var f = _currentFilter.ToLower();
            v = v.Replace(" ", "");
            f = f.Replace(" ", "");

            return v.Contains(f);
        }
    }
}