using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    public sealed partial class MainPage : Page
    {
        private NavigationItemKey _defaultPage = NavigationItemKey.Recommendations;
        private bool _pageLoaded = false;
        public MainPageViewModel ViewModel { get; }

        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
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
                NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
            }
        }

        private void NavView_Navigate(NavigationItemKey navItemTag, NavigationTransitionInfo transitionInfo)
        {
            if (ViewModel.Pages.ContainsKey(navItemTag))
            {
                var page = ViewModel.Pages.GetValueOrDefault(navItemTag);
                if (page is not null && !Equals(ContentFrame.CurrentSourcePageType, page))
                {
                    ContentFrame.Navigate(page, null, transitionInfo);
                }
            }
        }
    }
}
