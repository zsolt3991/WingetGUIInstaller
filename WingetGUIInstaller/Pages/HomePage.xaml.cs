using CommunityToolkit.Common.Extensions;
using CommunityToolkit.Helpers;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;
using Windows.Foundation;
using Windows.System;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Contracts;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Messages;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [NavigationKey(NavigationItemKey.Home)]
    public sealed partial class HomePage : Page
    {
        private readonly ISettingsStorageHelper<string> _applicationSettings;
        private readonly IMultiLevelNavigationService<NavigationItemKey> _navigationService;
        private NavigationItemKey _defaultPage;
        private NavigationItemKey? _currentTopLevelNavigationKey;

        public HomePageViewModel ViewModel { get; }

        public HomePage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
            Unloaded += MainPage_Unloaded;
            _navigationService = Ioc.Default.GetRequiredService<IMultiLevelNavigationService<NavigationItemKey>>();
            _navigationService.AddNavigationLevel(ContentFrame);
            ContentFrame.Navigated += ContentFrame_Navigated;
            _applicationSettings = Ioc.Default.GetRequiredService<ISettingsStorageHelper<string>>();
            _defaultPage = (NavigationItemKey)_applicationSettings
                .GetValueOrDefault(ConfigurationPropertyKeys.SelectedPage, ConfigurationPropertyKeys.SelectedPageDefaultValue);

            DataContext = ViewModel = Ioc.Default.GetRequiredService<HomePageViewModel>();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            WeakReferenceMessenger.Default.Register<NavigationRequestedMessage>(this, (r, m) =>
            {
                DispatcherQueue.TryEnqueue(() => RequestTopLevelNavigation(m.Value));
            });
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
            ContentFrame.Navigated -= ContentFrame_Navigated;
            _navigationService.RemoveNavigationLevel(ContentFrame);
        }

        private void MainPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            RequestTopLevelNavigation(_defaultPage);
        }

        private void RequestTopLevelNavigation(NavigationItemKey navigationItemKey,
            NavigationTransitionInfo transitionInfo = default, object args = default)
        {
            if (!IsLoaded)
            {
                _defaultPage = navigationItemKey;
                return;
            }

            if (_currentTopLevelNavigationKey == navigationItemKey)
            {
                return;
            }

            _navigationService.Navigate(navigationItemKey, transitionInfo, args, NavigationStackMode.Clear);
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (!TryGetNavigationItem(NavView, e.SourcePageType, out var navigationItem))
            {
                return;
            }

            if (Enum.TryParse<NavigationItemKey>(navigationItem.Tag?.ToString(), out var navigationItemKey))
            {
                _currentTopLevelNavigationKey = navigationItemKey;
            }

            if (ReferenceEquals(NavView.SelectedItem, navigationItem))
            {
                return;
            }

            SetSelectedItemWithoutNavigation(NavView, navigationItem, NavView_SelectionChnage);
        }

        private static bool TryGetNavigationItem(NavigationView navigationView, Type pageType, out NavigationViewItem navigationItem)
        {
            navigationItem = default;
            if (pageType == default)
            {
                return false;
            }

            var keyAttribute = pageType.GetCustomAttributes(typeof(NavigationKeyAttribute), false)
                .OfType<NavigationKeyAttribute>()
                .FirstOrDefault();
            if (keyAttribute == default)
            {
                return false;
            }

            var navigationItemKey = (NavigationItemKey)keyAttribute.NavigationItemKey;
            navigationItem = navigationView.MenuItems
                .Concat(navigationView.FooterMenuItems)
                .OfType<NavigationViewItem>()
                .FirstOrDefault(navItem => Enum.TryParse<NavigationItemKey>(navItem.Tag?.ToString(), out var navItemTag)
                    && navItemTag == navigationItemKey);

            return navigationItem != default;
        }

        private static void SetSelectedItemWithoutNavigation(NavigationView navigationView, NavigationViewItem navigationItem,
            TypedEventHandler<NavigationView, NavigationViewSelectionChangedEventArgs> selectionChangedHandler)
        {
            navigationView.SelectionChanged -= selectionChangedHandler;
            try
            {
                navigationView.SelectedItem = navigationItem;
            }
            finally
            {
                navigationView.SelectionChanged += selectionChangedHandler;
            }
        }

        private async void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.IsUpdateAvailable))
            {
                await UpdateDialog.ShowAsync().AsTask();
            }
        }

        private void NavView_SelectionChnage(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null &&
                Enum.TryParse<NavigationItemKey>(args.SelectedItemContainer.Tag.ToString(), out var navItemTag))
            {
                RequestTopLevelNavigation(navItemTag, args.RecommendedNavigationTransitionInfo);
            }
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            _navigationService.GoBack();
        }
    }
}
