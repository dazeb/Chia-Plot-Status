using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using ChiaPlotStatus.GUI.Models;
using ChiaPlotStatus.Logic.Models;
using ChiaPlotStatusGUI.GUI.Utils;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ChiaPlotStatus.Views
{
    public class DonationDialog : Window
    {
        public Language Language { get; set; }
        public string ChiaAddress { get; set; } = "xch1sxgmrrmq95klztd5796ysz8c6jattd6k70z4gxuet5a9792s24hqf4jdtn";
        public string PaypalURL { get; set; } = "https://www.paypal.com/donate?hosted_button_id=PDLLVF5XVMJPC";
        public string LiberapayURL { get; set; } = "https://liberapay.com/grayfallstown/donate";

        public DonationDialog() { }

        public DonationDialog(Language language)
        {
            this.DataContext = this;
            this.Language = language;
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.Find<TextBlock>("Thx").IsVisible = false;
            this.Focus();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void CopyToClipboard(object sender, RoutedEventArgs e)
        {
            string text = (string)(((Button)sender).Tag);
            Application.Current.Clipboard.SetTextAsync(text);
            this.Find<TextBlock>("Thx").IsVisible = true;
        }

        public void OpenLink(object sender, RoutedEventArgs e)
        {
            string url = (string)(((Button)sender).Tag);
            Utils.OpenUrl(url);
            this.Find<TextBlock>("Thx").IsVisible = true;
        }

    }
}