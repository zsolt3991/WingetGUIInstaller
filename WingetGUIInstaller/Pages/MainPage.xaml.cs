using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using WingetGUIInstaller.Constants;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Services;

namespace WingetGUIInstaller.Pages
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private readonly Dictionary<string, Type> _pages = new()
        {
            { "list", typeof(ListPage) },
            { "recommend", typeof(RecommendationsPage) },
            { "search", typeof(SearchPage) },
            { "update", typeof(UpgradePage) },
            { "settings", typeof(SettingsPage) },
            { "about", typeof(AboutPage) },
        };

        private bool _isConsoleEnabled;
        private ConfigurationStore _configurationStore;
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
            _configurationStore = Ioc.Default.GetRequiredService<ConfigurationStore>();
            _isConsoleEnabled = _configurationStore.GetStoredProperty(ConfigurationPropertyKeys.ConsoleEnabled, true);
            InitializeComponent();
            NavView.SelectedItem = NavView.MenuItems.FirstOrDefault(m => m is NavigationViewItem);
            WeakReferenceMessenger.Default.Register<ConsoleEnabledChangeMessage>(this, (r, m) =>
            {
                DispatcherQueue.TryEnqueue(() => IsConsoleEnabled = m.Value);
            });
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
            if (_pages.ContainsKey(navItemTag))
            {
                var page = _pages.GetValueOrDefault(navItemTag);
                if (page is not null && !Equals(ContentFrame.CurrentSourcePageType, page))
                {
                    ContentFrame.Navigate(page, null, transitionInfo);
                }
            }
        }
    }
}
