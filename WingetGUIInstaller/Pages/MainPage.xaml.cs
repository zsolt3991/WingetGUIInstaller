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
            
            WeakReferenceMessenger.Default.Register<NavigationRequestedMessage>(this, (r, m) =>
            {
                DispatcherQueue.TryEnqueue(() => NavView.SelectedItem =
                    NavView.MenuItems.FirstOrDefault(n => n is NavigationViewItem navItem &&
                    string.Equals(navItem.Tag.ToString(), m.Value.ToString(), StringComparison.InvariantCultureIgnoreCase)));
            });
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
                if (Enum.TryParse(args.SelectedItemContainer.Tag.ToString(), out NavigationItem navigationItem))
                {
                    NavView_Navigate(navigationItem, args.RecommendedNavigationTransitionInfo);
                }
            }
        }

        private void NavView_Navigate(NavigationItem navigationItem, NavigationTransitionInfo transitionInfo)
        {
            if (ViewModel.Pages.ContainsKey(navigationItem))
            {
                var page = ViewModel.Pages.GetValueOrDefault(navigationItem);
                if (page is not null && !Equals(ContentFrame.CurrentSourcePageType, page))
                {
                    ContentFrame.Navigate(page, null, transitionInfo);
                }
            }
        }
    }
}
