using FPV.Core;
using FPV.Service.Common;
using FPV.ViewModels;
using System;
using System.Collections.Generic;

namespace FPV.Service
{
    /// <summary>
    /// Service for navigating between different ViewModels in the application.
    /// </summary>
    public class NavigationService : ObservableObject, INavigationService
    {
        // Fields for storing the current view, content, section, article, and aside
        // These are updated by the NavigateTo methods and can be accessed by the ViewModels
        private ViewModelBase _currentView;
        private ViewModelBase _currentContent;
        private ViewModelBase _currentSection;
        private ViewModelBase _currentArticle;
        private ViewModelBase _currentAside;

        // The factory function used to create instances of ViewModelBase objects
        // This is injected via the constructor
        private readonly Func<Type, ViewModelBase> _viewModelFactory;
        private readonly Stack<ViewModelBase> _navigationStack;

        public NavigationService(Func<Type, ViewModelBase> viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
            _navigationStack = new Stack<ViewModelBase>();
        }

        // Properties for accessing the current view, content, section, article, and aside
        #region Properties
        public ViewModelBase CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ViewModelBase CurrentContent
        {
            get { return _currentContent; }
            set { _currentContent = value; OnPropertyChanged(); }
        }

        public ViewModelBase CurrentSection
        {
            get { return _currentSection; }
            set { _currentSection = value; OnPropertyChanged(); }
        }

        public ViewModelBase CurrentArticle
        {
            get { return _currentArticle; }
            set { _currentArticle = value; OnPropertyChanged(); }
        }

        public ViewModelBase CurrentAside
        {
            get { return _currentAside; }
            set { _currentAside = value; OnPropertyChanged(); }
        }

        #endregion

        /// <summary>
        /// Navigates to the specified ViewModel of type T.
        /// </summary>
        /// <typeparam name="T">Type of the ViewModel to navigate to.</typeparam>
        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            var viewModel = _viewModelFactory(typeof(TViewModel));

            if (viewModel is IHandleParameters handleParameters)
            {
                handleParameters.HandleParameters(null);
            }

            CurrentView = viewModel;
            _navigationStack.Push(viewModel);

            CurrentSection = null;
            CurrentContent = null;
        }

        /// <summary>
        /// Navigates to the specified ViewModel of type T with the given parameters.
        /// </summary>
        /// <typeparam name="T">Type of the ViewModel to navigate to.</typeparam>
        /// <param name="parameters">Parameters to pass to the ViewModel.</param>
        public void NavigateTo<TViewModel>(object parameter) where TViewModel : ViewModelBase
        {
            var viewModel = _viewModelFactory(typeof(TViewModel));

            if (viewModel is IHandleParameters handleParameters)
            {
                handleParameters.HandleParameters(parameter);
            }

            CurrentView = viewModel;
            _navigationStack.Push(viewModel);

            CurrentSection = null;
            CurrentContent = null;
        }

        /// <summary>
        /// Navigates to the specified ViewModel of type T with the given parameters.
        /// </summary>
        /// <typeparam name="T">Type of the ViewModel to navigate to.</typeparam>
        /// <param name="parameters">Parameters to pass to the ViewModel.</param>
        public void NavigateToContent<TViewModel>(object parameter) where TViewModel : ViewModelBase
        {
            var viewModel = _viewModelFactory(typeof(TViewModel));

            if (viewModel is IHandleParameters handleParameters)
            {
                handleParameters.HandleParameters(parameter);
            }

            CurrentContent = viewModel;
            CurrentSection = null;
        }
        public void NavigateToContent<TViewModel>() where TViewModel : ViewModelBase
        {
            CurrentContent = _viewModelFactory(typeof(TViewModel));
            CurrentSection = null;

        }

        /// <summary>
        /// Navigates to the specified ViewModel of type T with the given parameters.
        /// </summary>
        /// <typeparam name="T">Type of the ViewModel to navigate to.</typeparam>
        /// <param name="parameters">Parameters to pass to the ViewModel.</param>
        public void NavigateToSection<TViewModel>(object parameter) where TViewModel : ViewModelBase
        {
            var viewModel = _viewModelFactory(typeof(TViewModel));

            if (viewModel is IHandleParameters handleParameters)
            {
                handleParameters.HandleParameters(parameter);
            }

            CurrentSection = viewModel;
        }
        public void NavigateToSection<TViewModel>() where TViewModel : ViewModelBase
        {
            CurrentSection = _viewModelFactory(typeof(TViewModel));
        }

        public void NavigateToArticle<TViewModel>() where TViewModel : ViewModelBase
        {
            CurrentArticle = _viewModelFactory(typeof(TViewModel));
        }

        public void NavigateToAside<TViewModel>() where TViewModel : ViewModelBase
        {
            CurrentAside = _viewModelFactory(typeof(TViewModel));
        }

        /// <summary>
        /// Navigates back to the previous ViewModel on the navigation stack.
        /// </summary>
        public void GoBack()
        {
            if (_navigationStack.Count > 1)
            {
                _navigationStack.Pop();
                CurrentView = _navigationStack.Peek();
            }
        }

    }
}
