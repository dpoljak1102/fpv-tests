using FPV.ViewModels;

namespace FPV.Service.Common
{
    public interface INavigationService
    {
        ViewModelBase CurrentView { get; }
        ViewModelBase CurrentContent { get; set; }
        ViewModelBase CurrentSection { get; set; }
        ViewModelBase CurrentArticle { get; set; }
        ViewModelBase CurrentAside { get; set; }

        void NavigateTo<T>() where T : ViewModelBase;
        void NavigateTo<T>(object parameters) where T : ViewModelBase;
        void NavigateToContent<T>() where T : ViewModelBase;
        void NavigateToContent<T>(object parameters) where T : ViewModelBase;
        void NavigateToSection<T>() where T : ViewModelBase;
        void NavigateToSection<T>(object parameters) where T : ViewModelBase;
        void NavigateToArticle<T>() where T : ViewModelBase;
        void NavigateToAside<T>() where T : ViewModelBase;
        void GoBack();
    }
}
