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
    public sealed partial class VisualSettingPage : Page
    {
        StoreSettings setting;
        public VisualSettingPage()
        {
            this.InitializeComponent();
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += VisualSettingPage_BackRequested;
        }

        private void VisualSettingPage_BackRequested(object sender, BackRequestedEventArgs e)
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            setting = new StoreSettings();
            var tweetCount = setting.TryGetValueWithDefault("TweetCount", 50);
            int index;
            switch (tweetCount)
            {
                case 50:
                    index = 0;
                    break;
                case 100:
                    index = 1;
                    break;
                default:
                    index = 2;
                    break;
            }

            TweetCountBox.SelectedIndex = index;

            var textFontSize = setting.TryGetValueWithDefault("TextFontSize", 1);
            TextFontSizeBox.SelectedIndex = textFontSize;
        }

        private void TweetCountChanged(object sender, SelectionChangedEventArgs e)
        {

            ComboBoxItem count = (ComboBoxItem)((ComboBox)sender).SelectedValue;
            if (count != null)
            {
                setting.AddOrUpdateValue("TweetCount", int.Parse((string)count.Content));
            }
        }
        private void TextFontSizeChanged(object sender, SelectionChangedEventArgs e)
        {
            var index = ((ComboBox)sender).SelectedIndex;
            setting.AddOrUpdateValue("TextFontSize", index);
        }
    }
}
