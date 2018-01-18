using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using HotsBpHelper.Messages;
using HotsBpHelper.Utils;
using HotsBpHelper.Utils.ComboBoxItemUtil;
using Stylet;
using Size = System.Drawing.Size;

namespace HotsBpHelper.Pages
{
    public class HeroSelectorWindowViewModel : ViewModelBase
    {
        private Visibility _visibility = Visibility.Visible;
        private BindableCollection<HeroSelectorViewModel> m_heroSelectorViewModels;

        public HeroSelectorWindowViewModel()
        {
            m_heroSelectorViewModels = new BindableCollection<HeroSelectorViewModel>();
        }
        
        public BindableCollection<HeroSelectorViewModel> HeroSelectorViewModels
        {
            get { return m_heroSelectorViewModels; }
            set { SetAndNotify(ref m_heroSelectorViewModels, value); }
        }
        
    }
}