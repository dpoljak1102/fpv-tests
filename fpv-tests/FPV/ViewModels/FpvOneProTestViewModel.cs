using FPV.Core;
using FPV.Service.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FPV.ViewModels
{
    public class FpvOneProTestViewModel : ViewModelBase
    {
        public ICommand NavigateHomeCommand { get; set; }

        public FpvOneProTestViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;

            NavigateHomeCommand = new RelayCommand(o =>
            {
                NavigationService.NavigateTo<HomeViewModel>();
            });

        }
    }
}
