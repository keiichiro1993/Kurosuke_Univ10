using Kurosuke_Universal.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace Kurosuke_Universal.Pages.Settings
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += SettingsPage_BackRequested;
        }

        private void SettingsPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;

            if (DateTime.Now - TmpUserData.PreviousBackRequest < new TimeSpan(0, 0, 1))
            {
                return;
            }
            else
            {
                TmpUserData.PreviousBackRequest = DateTime.Now;
            }

            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }



        private void BackButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void Accounts(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AccountPage));
        }

        private void Appearance(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(VisualSettingPage));
        }

        private void Others(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(OtherSettingsPage));
        }
    }
}
