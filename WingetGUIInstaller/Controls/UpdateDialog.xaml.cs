﻿using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace WingetGUIInstaller.Controls
{
    public partial class UpdateDialog : ContentDialog
    {
        public static readonly DependencyProperty UpdateVersionProperty = DependencyProperty
            .Register("UpdateVersion", typeof(Version), typeof(UpdateDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty UpdateChangeLogProperty = DependencyProperty
            .Register("UpdateChangeLog", typeof(string), typeof(UpdateDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty UpdateAvailableProperty = DependencyProperty
            .Register("UpdateAvailable", typeof(bool), typeof(UpdateDialog), new PropertyMetadata(false));

        public Version UpdateVersion
        {
            get => (Version)GetValue(UpdateVersionProperty);
            set => SetValue(UpdateVersionProperty, value);
        }

        public string UpdateChangeLog
        {
            get => (string)GetValue(UpdateChangeLogProperty);
            set => SetValue(UpdateChangeLogProperty, value);
        }

        public bool UpdateAvailable
        {
            get => (bool)GetValue(UpdateAvailableProperty);
            set => SetValue(UpdateAvailableProperty, value);
        }

        public MarkdownConfig MarkdownConfig { get; } = new MarkdownConfig();

        public UpdateDialog()
        {
            InitializeComponent();
        }
    }
}
