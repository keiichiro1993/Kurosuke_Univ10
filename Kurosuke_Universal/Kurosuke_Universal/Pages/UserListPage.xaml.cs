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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace Kurosuke_Universal.Pages
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class UserListPage : Page
    {
        ObservableCollection<UserAccessToken> userAccessTokens;
        public UserListPage()
        {
            this.InitializeComponent();
        }
        private void _BackRequested(object sender, BackRequestedEventArgs e)
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
            Frame.GoBack();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += _BackRequested;

            UserListPageDataTransfer param = (UserListPageDataTransfer)e.Parameter;
            TwitterClient client;
            UserAccessToken token;
            switch (param.mode)
            {
                case UserListMode.Retweeted:
                    AdvancedTweet tweet = param.tweet;
                    client = new TwitterClient(tweet.accessToken);
                    userAccessTokens = await client.GetUserListFromTweets("https://api.twitter.com/1.1/statuses/retweets/" + tweet.tweet.id + ".json?count=100");
                    break;
                case UserListMode.Followed:
                    token = param.userAccessToken;
                    client = new TwitterClient(token.accessToken);
                    userAccessTokens = await client.GetUserListById("https://api.twitter.com/1.1/followers/ids.json?cursor=-1&screen_name=" + token.screenName + "&count=" + 100);
                    break;
                case UserListMode.Following:
                    token = param.userAccessToken;
                    client = new TwitterClient(token.accessToken);
                    userAccessTokens = await client.GetUserListById("https://api.twitter.com/1.1/friends/ids.json?cursor=-1&screen_name=" + token.screenName + " &count=" + 100);
                    break;
                case UserListMode.Favorited:
                    break;
            }

            accountList.DataContext = userAccessTokens;
        }

        private void AccountClicked(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserAccessToken user = (UserAccessToken)button.DataContext;

            var data = new UserAccessToken(user.accessToken, user.screenName);

            Frame.Navigate(typeof(UserDetail), data);
        }

        public enum UserListMode { Following, Followed, Retweeted, Favorited }

        public class UserListPageDataTransfer
        {
            public UserListPageDataTransfer(UserListMode mode, UserAccessToken userAccessToken)
            {
                this.mode = mode;
                this.userAccessToken = userAccessToken;
            }
            public UserListPageDataTransfer(UserListMode mode, AdvancedTweet tweet)
            {
                this.mode = mode;
                this.tweet = tweet;
            }

            public UserListMode mode;
            public UserAccessToken userAccessToken;
            public AdvancedTweet tweet;
        }
    }
}
