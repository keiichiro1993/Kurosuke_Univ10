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
using Windows.UI.Core;
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
    public sealed partial class TweetListPage : Page
    {
        public TweetColumn column;
        public TweetListPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += _BackRequested;

            var userAccessToken = (UserAccessToken)e.Parameter;
            column = new TweetColumn(userAccessToken.accessToken, userAccessToken.screenName);
            mainGrid.DataContext = column;
            await column.Run();
            var tokens = TmpUserData.Accounts;
            RetweetUserListBox.DataContext = tokens;
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

        private void QuoteTweet(object sender, RoutedEventArgs e)
        {
            var senderButton = (Button)sender;
            var tweet = (AdvancedTweet)senderButton.DataContext;
            Frame.Navigate(typeof(CreateTweetPage), new CreateTweetPage.CreateTweetPageTransfer(CreateTweetPage.CreateTweetPageTransferRole.QuotTweet, tweet));
        }
    }
}
