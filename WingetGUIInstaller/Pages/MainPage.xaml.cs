using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;
using Windows.System;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Services;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [PageKey(Enums.NavigationItemKey.Home)]
    public sealed partial class MainPage : Page
    {
        private NavigationItemKey _defaultPage = NavigationItemKey.Recommendations;
        private bool _pageLoaded = false;
        private readonly IMultiLevelNavigationService<NavigationItemKey> _navigationService;
        public MainPageViewModel ViewModel { get; }

        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
            Unloaded += MainPage_Unloaded;
            _navigationService = Ioc.Default.GetRequiredService<IMultiLevelNavigationService<NavigationItemKey>>();
            _navigationService.AddNavigationLevel(ContentFrame);
            DataContext = ViewModel = Ioc.Default.GetRequiredService<MainPageViewModel>();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            WeakReferenceMessenger.Default.Register(this, (MessageHandler<object, NavigationRequestedMessage>)((r, m) =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    if (!_pageLoaded)
                    {
                        _defaultPage = m.Value;
                    }
                    else
                    {
                        ChangeSelectedItem(m.Value);
                    }
                });
            }));
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                _navigationService.RemoveNavigationLevel(ContentFrame);
            }
        }

        private void MainPage_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            _navigationService.RemoveNavigationLevel(ContentFrame);
        }

        private void MainPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            _pageLoaded = true;
            // Select the first clickable item when the page is shown
            ChangeSelectedItem(_defaultPage);
        }

        private void ChangeSelectedItem(NavigationItemKey navigationItemKey)
        {
            NavView.SelectedItem =
                NavView.MenuItems.FirstOrDefault(n => n is NavigationViewItem navItem &&
                string.Equals(navItem.Tag.ToString(), navigationItemKey.ToString(), StringComparison.InvariantCultureIgnoreCase));
        }

        private async void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.IsUpdateAvailable))
            {
                if (ViewModel.IsUpdateAvailable)
                {
                    await UpdateDialog.ShowAsync().AsTask();
                }
            }
        }

        private void NavView_SelectionChnage(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null &&
                Enum.TryParse<NavigationItemKey>(args.SelectedItemContainer.Tag.ToString(), out var navItemTag))
            {
                _navigationService.Navigate(navItemTag, args.RecommendedNavigationTransitionInfo, null);
            }
        }
    }
}
