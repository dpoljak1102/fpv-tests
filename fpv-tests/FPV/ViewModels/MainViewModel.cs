using FPV.Core;
using FPV.Service.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FPV.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ICommand CloseWindowCommand { get; set; }
        public ICommand MaximizeWindowCommand { get; set; }
        public ICommand MinimizeWindowCommand { get; set; }

        public bool IsMaximized { get; set; } = false;


        public MainViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;
            NavigationService.NavigateTo<HomeViewModel>();


            CloseWindowCommand = new RelayCommand(o =>
            {
                // Check if user is login view model then close windows app
                Window win = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.Name == "MainWindowName");
                if(win != null)
                    win.Close();
            });


            MaximizeWindowCommand = new RelayCommand(o =>
            {
                // Check if user is login view model then close windows app
                Window win = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.Name == "MainWindowName");
                if (win != null)
                {
                    if (IsMaximized)
                    {
                        win.WindowState = WindowState.Normal;
                        IsMaximized = false;
                    }
                    else
                    {
                        win.WindowState = WindowState.Maximized;
                        IsMaximized = true;
                    }
                }
            });

            MinimizeWindowCommand = new RelayCommand(o =>
            {
                // Check if user is login view model then close windows app
                Window win = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.Name == "MainWindowName");
                if (win != null)
                {
                    win.WindowState = WindowState.Minimized;
                }
            });


        }

    }
}
