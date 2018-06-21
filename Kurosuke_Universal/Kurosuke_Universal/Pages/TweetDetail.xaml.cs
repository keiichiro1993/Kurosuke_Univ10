using Kurosuke_Universal.Models;
using Kurosuke_Universal.Utils;
using Kurosuke_Universal.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.UI.Core;
using System.Linq;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace Kurosuke_Universal.Pages
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class TweetDetail : Page
    {
        public AdvancedTweet tweet;
        public ObservableCollection<AdvancedTweet> replyTweets;
        public ObservableCollection<UserAccessToken> tokens;
        public TweetDetail()
        {
            this.InitializeComponent();
        }

        private void BackButtonClicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var data = (AdvancedTweet)e.Parameter;
            tweet = data;
            Init();

            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += _BackRequested;

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

        private async void Init()
        {
            tokens = TmpUserData.Accounts;
            RetweetUserListBox.DataContext = tokens;

            var client = new TwitterClient(tweet.accessToken);
            try
            {
                tweet = await client.GetTweet(tweet.tweet.id_str);
                mainTweetGrid.DataContext = tweet;
                replyTweets = new ObservableCollection<AdvancedTweet>();
                RelatedTweetsItemsControl.DataContext = replyTweets;
                GetReplys(tweet);
            }
            catch (Exception ex)
            {
                var message = new MessageDialog(ex.Message);
                await message.ShowAsync();
                return;
            }
        }

        private async void GetReplys(AdvancedTweet sourceTweet)
        {
            if (!string.IsNullOrEmpty(sourceTweet.tweet.in_reply_to_status_id_str))
            {
                try
                {
                    var client = new TwitterClient(tweet.accessToken);
                    var reply = await client.GetTweet(sourceTweet.tweet.in_reply_to_status_id_str);
                    replyTweets.Add(reply);
                    GetReplys(reply);
                }
                catch (Exception ex)
                {
                    var message = new MessageDialog(ex.Message);
                    await message.ShowAsync();
                    return;
                }
            }
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


        private void UserClicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(UserDetail), new UserAccessToken(tweet.accessToken, tweet.tweet.user.screen_name));
        }

        private void ReplyButtonClicked(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var tweet = (AdvancedTweet)button.DataContext;
            Frame.Navigate(typeof(CreateTweetPage), new CreateTweetPage.CreateTweetPageTransfer(CreateTweetPage.CreateTweetPageTransferRole.Reply, tweet));
        }

        private void TweetItemClicked(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var tweet = (AdvancedTweet)button.DataContext;
            Frame.Navigate(typeof(TweetDetail), tweet);
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

        private void RetweetedCountClicked(object sender, RoutedEventArgs e)
        {
            if (tweet.tweet.retweet_count != 0)
            {
                var data = new UserListPage.UserListPageDataTransfer(UserListPage.UserListMode.Retweeted, tweet);
                Frame.Navigate(typeof(UserListPage), data);
            }
        }

        private void QuoteTweet(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CreateTweetPage), new CreateTweetPage.CreateTweetPageTransfer(CreateTweetPage.CreateTweetPageTransferRole.QuotTweet, tweet));
        }
    }
}
