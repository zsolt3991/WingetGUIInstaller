using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WingetGUIInstaller.Controls
{
    public sealed partial class CustomContentSettingsControl : UserControl
    {
        public static readonly DependencyProperty PrimaryTextProperty = DependencyProperty
            .Register("PrimaryText", typeof(string), typeof(CustomContentSettingsControl), new PropertyMetadata(null));

        public static readonly DependencyProperty SecondaryTextProperty = DependencyProperty
            .Register("SecondaryText", typeof(string), typeof(CustomContentSettingsControl), new PropertyMetadata(null));

        public static readonly DependencyProperty CustomContentProperty = DependencyProperty
            .Register("CustomContent", typeof(FrameworkElement), typeof(CustomContentSettingsControl), new PropertyMetadata(null));

        public string PrimaryText
        {
            get => (string)GetValue(PrimaryTextProperty);
            set => SetValue(PrimaryTextProperty, value);
        }

        public string SecondaryText
        {
            get => (string)GetValue(SecondaryTextProperty);
            set => SetValue(SecondaryTextProperty, value);
        }

        public FrameworkElement CustomContent
        {
            get => (FrameworkElement)GetValue(CustomContentProperty);
            set => SetValue(CustomContentProperty, value);
        }

        public CustomContentSettingsControl()
        {
            InitializeComponent();
        }
    }
}
