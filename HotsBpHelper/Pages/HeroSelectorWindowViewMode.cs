using System.Windows;
using Stylet;

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