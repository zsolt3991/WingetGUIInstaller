using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Models;

namespace WingetGUIInstaller.Pages
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private readonly ApplicationDataStorageHelper _configurationStore;
        private readonly Dictionary<NavigationItem, Type> _pages = new()
        {
            { NavigationItem.InstalledPackages, typeof(ListPage) },
            { NavigationItem.Recommendations, typeof(RecommendationsPage) },
            { NavigationItem.Search, typeof(SearchPage) },
            { NavigationItem.Upgrades, typeof(UpgradePage) },
            { NavigationItem.Settings, typeof(SettingsPage) },
            { NavigationItem.Console, typeof(ConsolePage) },
            { NavigationItem.About, typeof(AboutPage) },
        };

        private bool _isConsoleEnabled;
        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsConsoleEnabled
        {
            get => _isConsoleEnabled;
            set
            {
                _isConsoleEnabled = value;
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(IsConsoleEnabled)));
            }
        }

        public MainPage()
        {
            _configurationStore = Ioc.Default.GetRequiredService<ApplicationDataStorageHelper>();
            _isConsoleEnabled = _configurationStore.Read(ConfigurationPropertyKeys.ConsoleEnabled,
                ConfigurationPropertyKeys.ConsoleEnabledDefaultValue);
            InitializeComponent();
            NavView.SelectedItem = NavView.MenuItems.FirstOrDefault(m => m is NavigationViewItem);

            WeakReferenceMessenger.Default.Register<ConsoleEnabledChangeMessage>(this, (r, m) =>
            {
                DispatcherQueue.TryEnqueue(() => IsConsoleEnabled = m.Value);
            });

            WeakReferenceMessenger.Default.Register<NavigationRequestedMessage>(this, (r, m) =>
            {
                DispatcherQueue.TryEnqueue(() => NavView.SelectedItem =
                    NavView.MenuItems.FirstOrDefault(n => n is NavigationViewItem navItem &&
                    string.Equals(navItem.Tag.ToString(), m.Value.ToString(), StringComparison.InvariantCultureIgnoreCase)));
            });
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
            if (_pages.ContainsKey(navigationItem))
            {
                var page = _pages.GetValueOrDefault(navigationItem);
                if (page is not null && !Equals(ContentFrame.CurrentSourcePageType, page))
                {
                    ContentFrame.Navigate(page, null, transitionInfo);
                }
            }
        }
    }
}
