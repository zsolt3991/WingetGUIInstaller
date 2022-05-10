using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WingetGUIInstaller.Utils;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    [PageKey(Enums.NavigationItemKey.Console)]
    public sealed partial class ConsolePage : Page
    {
        public ConsolePage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            DataContext = ViewModel = Ioc.Default.GetRequiredService<ConsolePageViewModel>();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.ComposedMessage))
            {
                OutputScroll.UpdateLayout();
                OutputScroll.ScrollToVerticalOffset(OutputScroll.ScrollableHeight + 50);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            OutputScroll.UpdateLayout();
            OutputScroll.ScrollToVerticalOffset(OutputScroll.ScrollableHeight + 50);
        }

        public ConsolePageViewModel ViewModel { get; }
    }
}
