using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Windows.Input;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Controls
{
    [ObservableObject]
    public sealed partial class PackageDetailsView : UserControl
    {
        private Visibility _moreButtonVisibility = Visibility.Collapsed;

        public static readonly DependencyProperty PackageDetailsProperty = DependencyProperty
              .Register("PackageDetails", typeof(PackageDetailsViewModel), typeof(PackageDetailsView), new PropertyMetadata(null));

        public static readonly DependencyProperty MoreButtonCommandProperty = DependencyProperty
           .Register("MoreButtonCommand", typeof(ICommand), typeof(PackageDetailsView), new PropertyMetadata(null, MoreButtonPropertyChanged));

        public PackageDetailsView()
        {
            InitializeComponent();
        }

        public PackageDetailsViewModel PackageDetails
        {
            get => GetValue(PackageDetailsProperty) as PackageDetailsViewModel;
            set => SetValue(PackageDetailsProperty, value);
        }

        public ICommand MoreButtonCommand
        {
            get => GetValue(MoreButtonCommandProperty) as ICommand;
            set => SetValue(MoreButtonCommandProperty, value);
        }

        // Not attached to Dependency Property
        public Visibility MoreButtonVisibility
        {
            get => _moreButtonVisibility;
            set => SetProperty(ref _moreButtonVisibility, value);
        }

        private static void MoreButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as PackageDetailsView;
            if (control != default)
            {
                if (e.NewValue != default)
                {
                    control.MoreButtonVisibility = Visibility.Visible;
                }
                else
                {
                    control.MoreButtonVisibility = Visibility.Collapsed;
                }
            }
        }
    }
}
