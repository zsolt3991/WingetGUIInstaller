using Microsoft.UI.Xaml.Navigation;

namespace WingetGUIInstaller.Contracts;

public interface INavigationAware
{
    void OnNavigatedTo(object parameter);

    void OnNavigatedFrom(NavigationMode navigationMode);
}
