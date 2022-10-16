using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WingetGUIInstaller.Controls
{
    public sealed partial class LoadingIndicator : UserControl
    {
        public static readonly DependencyProperty MessageProperty = DependencyProperty
            .Register("Message", typeof(string), typeof(LoadingIndicator), new PropertyMetadata(null));

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty
            .Register("IsLoading", typeof(bool), typeof(LoadingIndicator), new PropertyMetadata(false));

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public LoadingIndicator()
        {
            InitializeComponent();
        }
    }
}
