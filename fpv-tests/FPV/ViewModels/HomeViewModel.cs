using FPV.Core;
using FPV.Models;
using FPV.Service.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FPV.ViewModels
{
    public class HomeViewModel : ViewModelBase, IHandleParameters
    {
        #region Properties
        private ObservableCollection<ProductCardModel> _productModelCollection;

        public ObservableCollection<ProductCardModel> ProductModelCollection
        {
            get { return _productModelCollection; }
            set { _productModelCollection = value; OnPropertyChanged(); }
        }

        private ProductCardModel _selectedModel;
        public ProductCardModel SelectedModel
        {
            get { return _selectedModel; }
            set { _selectedModel = value; OnPropertyChanged(); OnSelectedCard();}
        }
        private void OnSelectedCard()
        {
            // Selected card
            if (SelectedModel?.Name != string.Empty)
            {
                if (SelectedModel.Name.Contains("FPV.ONE PILOT"))
                {
                    // Handler for Pilot
                    //NavigationService.NavigateToContent<DashboardViewModel>();
                }
                if (SelectedModel.Name.Contains("FPV.ONE RACE"))
                {
                    // Handler for Race
                    //NavigationService.NavigateToContent<DashboardViewModel>();
                }
                if (SelectedModel.Name.Contains("FPV.ONE PRO"))
                {
                    //Handler for pro
                    NavigationService.NavigateTo<FpvOneProTestViewModel>();

                }
            }
        }

        #endregion

        public HomeViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;

            ProductCardModel pilot = new ProductCardModel{Name = "FPV.ONE PILOT", ImageSource = "../Assets/Images/fpv-one-pilot-card.png" };
            ProductCardModel race = new ProductCardModel { Name = "FPV.ONE RACE/FLYE FILE 533", ImageSource = "../Assets/Images/fpv-one-race-card.png" };
            ProductCardModel pro = new ProductCardModel { Name = "FPV.ONE PRO", ImageSource = "../Assets/Images/fpv-one-pro-card.png" };

            ProductModelCollection = new ObservableCollection<ProductCardModel>();
            ProductModelCollection.Add(pilot);
            ProductModelCollection.Add(race);
            ProductModelCollection.Add(pro);


        }

        public void HandleParameters(object parameters)
        {
            // Here you can convert the parameter to the desired data type and use it in the ViewModel
        }

    }
}
