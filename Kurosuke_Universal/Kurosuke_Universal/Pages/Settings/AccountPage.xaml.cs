using Kurosuke_Universal.Models;
using Kurosuke_Universal.Utils;
using Kurosuke_Universal.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
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
    public sealed partial class AccountPage : Page
    {
        public ObservableCollection<UserAccessToken> userAccessTokens;
        private ObservableCollection<AccessToken> tokens;
        public AccountPage()
        {
            this.InitializeComponent();
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += AccountPage_BackRequested;
        }

        private void AccountPage_BackRequested(object sender, BackRequestedEventArgs e)
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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var storage = new StoreSettings();
            tokens = storage.TryGetValueWithDefault<ObservableCollection<AccessToken>>("AccessTokens", null);

            if (tokens != null)
            {
                userAccessTokens = new ObservableCollection<UserAccessToken>();
                foreach (var token in tokens)
                {
                    userAccessTokens.Add(new UserAccessToken(token));
                }
                accountList.DataContext = userAccessTokens;
            }

        }

        private void AddAccount(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AuthorizePage));
        }

        private void BackButtonClicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void DeleteAccount(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var user = (UserAccessToken)button.DataContext;
            var screenName = user.accessToken.screenName;

            var storage = new StoreSettings();

            try
            {
                var item = from token in tokens
                           where token.screenName == screenName
                           select token;

                tokens.Remove(item.First());

                storage.AddOrUpdateValue("AccessTokens", tokens);

                MessageDialog success = new MessageDialog("アカウント: @" + screenName + "が削除されました。");
                await success.ShowAsync();

                Frame.Navigate(typeof(AccountPage));
            }
            catch (Exception ex)
            {
                MessageDialog message = new MessageDialog("アカウントを削除できませんでした。:" + ex.Message, "おや？何かがおかしいようです。");
                await message.ShowAsync();
            }
        }

        private void AccountClicked(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserAccessToken user = (UserAccessToken)button.DataContext;

            var data = new UserAccessToken(user.accessToken, user.accessToken.screenName);

            Frame.Navigate(typeof(UserDetail), data);
        }
    }
}
