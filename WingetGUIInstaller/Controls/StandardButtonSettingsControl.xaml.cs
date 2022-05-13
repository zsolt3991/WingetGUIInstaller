using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Windows.Input;

namespace WingetGUIInstaller.Controls
{
    public sealed partial class StandardButtonSettingsControl : UserControl
    {
        public static readonly DependencyProperty PrimaryTextProperty = DependencyProperty
            .Register("PrimaryText", typeof(string), typeof(StandardButtonSettingsControl), new PropertyMetadata(null));

        public static readonly DependencyProperty SecondaryTextProperty = DependencyProperty
            .Register("SecondaryText", typeof(string), typeof(StandardButtonSettingsControl), new PropertyMetadata(null));

        public static readonly DependencyProperty ButtonCommandProperty = DependencyProperty
            .Register("ButtonCommand", typeof(ICommand), typeof(StandardButtonSettingsControl), new PropertyMetadata(null));

        public static readonly DependencyProperty ButtonTextProperty = DependencyProperty
            .Register("ButtonText", typeof(string), typeof(StandardButtonSettingsControl), new PropertyMetadata(null));

        public ICommand ButtonCommand
        {
            get { return (ICommand)GetValue(ButtonCommandProperty); }
            set { SetValue(ButtonCommandProperty, value); }
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

        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public StandardButtonSettingsControl()
        {
            InitializeComponent();
        }
    }
}
