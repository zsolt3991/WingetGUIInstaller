using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Pages
{
    public sealed partial class ConsolePage : Page
    {
        public ConsolePage()
        {
            NavigationCacheMode = Microsoft.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            DataContext = ViewModel = Ioc.Default.GetRequiredService<ConsolePageViewModel>();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            InitializeComponent();
            OutputScroll.ScrollToVerticalOffset(OutputScroll.ScrollableHeight);
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.ComposedMessage))
            {
                OutputScroll.ScrollToVerticalOffset(OutputScroll.ScrollableHeight);
            }
        }

        public ConsolePageViewModel ViewModel { get; }
    }
}
