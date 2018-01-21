using Stylet;

namespace HotsBpHelper.Pages
{
    public class HeroSelectorWindowViewModel : ViewModelBase
    {
        private BindableCollection<HeroSelectorViewModel> _HeroSelectorViewModels;

        public HeroSelectorWindowViewModel()
        {
            _HeroSelectorViewModels = new BindableCollection<HeroSelectorViewModel>();
        }

        public BindableCollection<HeroSelectorViewModel> HeroSelectorViewModels
        {
            get { return _HeroSelectorViewModels; }
            set { SetAndNotify(ref _HeroSelectorViewModels, value); }
        }
    }
}