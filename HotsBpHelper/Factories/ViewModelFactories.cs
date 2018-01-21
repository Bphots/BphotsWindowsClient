using HotsBpHelper.Pages;

namespace HotsBpHelper.Factories
{
    public interface IViewModelFactory
    {}

    public interface IHeroSelectorViewModelFactory : IViewModelFactory
    {
        HeroSelectorViewModel CreateViewModel();
    }

    public interface IHeroSelectorWindowViewModelFactory : IViewModelFactory
    {
        HeroSelectorWindowViewModel CreateViewModel();
    }

    public interface IMapSelectorViewModelFactory : IViewModelFactory
    {
        MapSelectorViewModel CreateViewModel();
    }

    public interface IBpViewModelFactory : IViewModelFactory
    {
        BpViewModel CreateViewModel();
    }

    public interface IWebFileUpdaterViewModelFactory : IViewModelFactory
    {
        WebFileUpdaterViewModel CreateViewModel();
    }

    public interface IMMRViewModelFactory : IViewModelFactory
    {
        MMRViewModel CreateViewModel();
    }
}