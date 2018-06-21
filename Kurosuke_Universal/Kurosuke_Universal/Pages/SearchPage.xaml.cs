using Kurosuke_Universal.Models;
using Kurosuke_Universal.Utils;
using Kurosuke_Universal.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace Kurosuke_Universal.Pages
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        TweetColumn column;
        ObservableCollection<UserAccessToken> users;
        int userIndex;
        public SearchPage()
        {
            this.InitializeComponent();
            RetweetUserListBox.DataContext = users;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += _BackRequested;
            var setting = new StoreSettings();

            var tokens = setting.TryGetValueWithDefault<ObservableCollection<AccessToken>>("AccessTokens", null);
            if (tokens != null)
            {
                if (tokens.Count != 0)
                {
                    users = TmpUserData.Accounts;
                    SearchUserSelect.DataContext = users;

                    userIndex = setting.TryGetValueWithDefault("SearchPage_UserIndex", 0);
                    SearchUserSelect.SelectedItem = users[userIndex];
                }
                else
                {
                    var message = new MessageDialog("まずは設定からアカウントを登録する必要があります。", "おや？なにかがおかしいようです。");
                    await message.ShowAsync();
                }
            }
        }

        private void _BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
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


        private AdvancedTweet tmpRetweet;
        private void RetweetButtonClicked(object sender, RoutedEventArgs e)
        {
            AdvancedTweet tweet = (AdvancedTweet)((Button)sender).DataContext;
            tmpRetweet = tweet;
            var items = RetweetUserListBox.Items;
            var select = from item in items
                         where ((UserAccessToken)item).screenName == tmpRetweet.accessToken.screenName
                         select item as UserAccessToken;

            RetweetUserListBox.SelectedItem = select.First();
            RetweetFlyout.ShowAt((Button)sender);
        }

        private async void RetweetCommand(object sender, RoutedEventArgs e)
        {
            RetweetFlyout.Hide();
            Tweet tweet;
            if (tmpRetweet.state == AdvancedTweet.States.Retweeted)
            {
                tweet = tmpRetweet.source;
            }
            else
            {
                tweet = tmpRetweet.tweet;
            }
            var users = RetweetUserListBox.SelectedItems;
            foreach (var user in users)
            {
                await Retweet(tweet, (UserAccessToken)user);
            }
        }

        private async Task Retweet(Tweet tweet, UserAccessToken user)
        {
            var client = new TwitterClient(user.accessToken);
            HttpResponseMessage res;
            try
            {
                res = await client.ChangeRetweet(tweet);
                if (!res.IsSuccessStatusCode)
                {
                    throw new Exception(res.ReasonPhrase);
                }
                else
                {
                    tmpRetweet.retweetedImageUrl = "/Assets/TwitterIcons/retweet_on.png";
                    tweet.retweeted = true;
                }
            }
            catch (Exception ex)
            {
                //throw new Exception("おや？　なにかがおかしいようです。", ex);
                var message = new MessageDialog(ex.Message, "おや？ なにかがおかしいようです。");
                await message.ShowAsync();
            }
        }

        private void ReplyButtonClicked(object sender, RoutedEventArgs e)
        {
            var replyTweet = (AdvancedTweet)(((Button)sender).DataContext);
            Frame.Navigate(typeof(CreateTweetPage), new CreateTweetPage.CreateTweetPageTransfer(CreateTweetPage.CreateTweetPageTransferRole.Reply, replyTweet));
        }

        private void UrlsClicked(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var item = (Url)button.DataContext;
            var uri = new Uri(item.url);

            Helpers.OpenBrowser(Frame, uri);
        }

        private void MediaUrlClicked(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var item = (Media)button.DataContext;
            var uri = new Uri(item.url);
            Helpers.OpenBrowser(Frame, uri);
        }

        private void GoToDetail(object sender, RoutedEventArgs e)
        {
            var senderButton = (Button)sender;
            var tweet = (AdvancedTweet)senderButton.DataContext;
            Frame.Navigate(typeof(Pages.TweetDetail), tweet);
        }

        private async void SearchButtonClicked(object sender, RoutedEventArgs e)
        {
            var word = SearchWordBox.Text;
            if (!string.IsNullOrEmpty(word))
            {
                StoreSettings setting = new StoreSettings();
                var count = setting.TryGetValueWithDefault("TweetCount", 50);
                column = new TweetColumn(users[SearchUserSelect.SelectedIndex].accessToken, TweetColumn.ColumnRoles.Search);
                TweetListItemsControl.DataContext = column.tweetList;
                try
                {
                    await column.GetSearchResultTimeline(word, count, null);
                }
                catch (Exception ex)
                {
                    var message = new MessageDialog(ex.Message, "おや？何かがおかしいようです。");
                    await message.ShowAsync();
                }
            }
            else
            {

            }
        }
    }
}
