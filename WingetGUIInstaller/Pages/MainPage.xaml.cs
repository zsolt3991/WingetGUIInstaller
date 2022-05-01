using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    public sealed partial class MainPage : Page
    {
        public MainPageViewModel ViewModel { get; }

        public MainPage()
        {
            InitializeComponent();
            DataContext = ViewModel = Ioc.Default.GetRequiredService<MainPageViewModel>();
            NavView.SelectedItem = NavView.MenuItems.FirstOrDefault(m => m is NavigationViewItem);
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private async void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(ViewModel.IsUpdateAvailable))
            {
                if(ViewModel.IsUpdateAvailable)
                {
                    await UpdateDialog.ShowAsync().AsTask();
                }
            }
        }

        private void NavView_SelectionChnage(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer != null)
            {
                var navItemTag = args.SelectedItemContainer.Tag.ToString();
                NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
            }
        }

        private void NavView_Navigate(string navItemTag, NavigationTransitionInfo transitionInfo)
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
