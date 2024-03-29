﻿using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WingetGUIInstaller.Contracts;
using WingetGUIInstaller.Enums;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [NavigationKey(NavigationItemKey.Recommendations)]
    public sealed partial class RecommendationsPage : Page
    {
        private readonly INavigationService<NavigationItemKey> _navigationService;

        public RecommendationsPageViewModel ViewModel { get; }

        public RecommendationsPage()
        {
            _navigationService = Ioc.Default.GetRequiredService<INavigationService<NavigationItemKey>>();
            DataContext = ViewModel = Ioc.Default.GetRequiredService<RecommendationsPageViewModel>();
            InitializeComponent();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is RecommendedItemViewModel recommendedItem)
            {
                var availableOperations = recommendedItem.IsInstalled ? AvailableOperation.Uninstall : AvailableOperation.Install;
                if (recommendedItem.HasUpdate)
                {
                    availableOperations |= AvailableOperation.Update;
                }

                _navigationService.Navigate(NavigationItemKey.PackageDetails, args: new PackageDetailsNavigationArgs
                {
                    AvailableOperation = availableOperations,
                    PackageId = recommendedItem.Id,
                });
            }
        }
    }
}
