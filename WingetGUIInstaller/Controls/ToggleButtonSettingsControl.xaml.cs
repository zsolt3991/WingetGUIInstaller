using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;


namespace WingetGUIInstaller.Controls
{
    public sealed partial class ToggleButtonSettingsControl : UserControl
    {
        public static readonly DependencyProperty PrimaryTextProperty = DependencyProperty
                  .Register("PrimaryText", typeof(string), typeof(ToggleButtonSettingsControl), new PropertyMetadata(null));

        public static readonly DependencyProperty SecondaryTextProperty = DependencyProperty
            .Register("SecondaryText", typeof(string), typeof(ToggleButtonSettingsControl), new PropertyMetadata(null)); 
        
        public static readonly DependencyProperty IsOnProperty = DependencyProperty
            .Register("IsOn", typeof(bool), typeof(ToggleButtonSettingsControl), new PropertyMetadata(false));

        public bool IsOn
        {
            get { return (bool)GetValue(IsOnProperty); }
            set { SetValue(IsOnProperty, value); }
        }

        public string PrimaryText
        {
            get { return (string)GetValue(PrimaryTextProperty); }
            set { SetValue(PrimaryTextProperty, value); }
        }

        public string SecondaryText
        {
            get { return (string)GetValue(SecondaryTextProperty); }
            set { SetValue(SecondaryTextProperty, value); }
        }

        public ToggleButtonSettingsControl()
        {
            InitializeComponent();
        }
    }
}
