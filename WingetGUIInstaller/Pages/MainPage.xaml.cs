using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    public sealed partial class MainPage : Page
    {
        public MainPageViewModel ViewModel { get; }

        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
            DataContext = ViewModel = Ioc.Default.GetRequiredService<MainPageViewModel>();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void MainPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Select the first clickable item when the page is shown
            NavView.SelectedItem = NavView.MenuItems.FirstOrDefault(m => m is NavigationViewItem);
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
