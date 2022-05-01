using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.System;

namespace WingetGUIInstaller.Controls
{
    public partial class UpdateDialog : ContentDialog
    {
        public static readonly DependencyProperty UpdateVersionProperty = DependencyProperty
               .Register("UpdateVersion", typeof(Version), typeof(LoadingIndicator), new PropertyMetadata(null));

        public static readonly DependencyProperty UpdateChangeLogProperty = DependencyProperty
            .Register("UpdateChangeLog", typeof(string), typeof(LoadingIndicator), new PropertyMetadata(null));

        public Version UpdateVersion
        {
            get { return (Version)GetValue(UpdateVersionProperty); }
            set { SetValue(UpdateVersionProperty, value); }
        }

        public string UpdateChangeLog
        {
            get { return (string)GetValue(UpdateChangeLogProperty); }
            set { SetValue(UpdateChangeLogProperty, value); }
        }

        public UpdateDialog()
        {
            InitializeComponent();
        }

        private async void MarkdownTextBlock_LinkClicked(object sender, CommunityToolkit.WinUI.UI.Controls.LinkClickedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(e.Link));
        }
    }
}
