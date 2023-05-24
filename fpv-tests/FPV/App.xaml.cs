using FPV.Service;
using FPV.Service.Common;
using FPV.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace FPV
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            IServiceCollection service = new ServiceCollection();

            service.AddSingleton<MainWindow>(provider => new MainWindow
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            });

            service.AddSingleton<MainViewModel>();
            service.AddTransient<HomeViewModel>();

            service.AddTransient<FpvOnePilotTestViewModel>();
            service.AddTransient<FpvOneRaceTestViewModel>();
            service.AddTransient<FpvOneProTestViewModel>();

            service.AddSingleton<INavigationService, NavigationService>();
            service.AddSingleton<Func<Type, ViewModelBase>>(serviceProvider => viewModelType => (ViewModelBase)serviceProvider.GetRequiredService(viewModelType));

            _serviceProvider = service.BuildServiceProvider();
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            base.OnStartup(e);
        }

    }

   
}
