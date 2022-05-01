using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;


namespace WingetGUIInstaller.Controls
{
    public sealed partial class PackageSourceDialog : ContentDialog
    {
        public static readonly DependencyProperty PackageSourceNameProperty = DependencyProperty
             .Register("PackageSourceName", typeof(string), typeof(PackageSourceDialog), new PropertyMetadata(null)); 
        
        public static readonly DependencyProperty PackageSourceUrlProperty = DependencyProperty
           .Register("PackageSourceUrl", typeof(string), typeof(PackageSourceDialog), new PropertyMetadata(null));

        public string PackageSourceName
        {
            get { return (string)GetValue(PackageSourceNameProperty); }
            set { SetValue(PackageSourceNameProperty, value); }
        }

        public string PackageSourceUrl
        {
            get { return (string)GetValue(PackageSourceUrlProperty); }
            set { SetValue(PackageSourceUrlProperty, value); }
        }

        public PackageSourceDialog()
        {
            InitializeComponent();
        }
    }
}
