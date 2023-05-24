using FPV.Core;
using FPV.Service.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPV.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {
        #region SERVICES
        private INavigationService _navigationService;

        public INavigationService NavigationService
        {
            get { return _navigationService; }
            set { _navigationService = value; OnPropertyChanged(); }
        }
        #endregion
    }
}
