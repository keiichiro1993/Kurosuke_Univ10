using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Kurosuke_Universal.Models;
using Windows.UI.Popups;
using Kurosuke_Universal.ViewModels;
using System.Threading.Tasks;
using Kurosuke_Universal.Utils;
using System.Collections.ObjectModel;
using Windows.Web.Http;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml.Input;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Kurosuke_Universal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ResourceLoader loader;
        public MainPageViewModel viewModel;
        public MainPage()
        {
            this.InitializeComponent();
            loader = new ResourceLoader();
        }

        private async void Init()
        {
            TweetTextBox.TweetButton = TweetButton;
            TweetTextBox.TweetUserList = TweetUserList;

            TmpUserData.PreviousBackRequest = DateTime.Now;

            var storage = new StoreSettings();
            var tokens = storage.TryGetValueWithDefault<ObservableCollection<AccessToken>>("AccessTokens", null);
            if (tokens != null)
            {
                if (tokens.Count != 0)
                {
                    viewModel = new MainPageViewModel(tokens);
                    MainGrid.DataContext = viewModel;
                    RetweetUserListBox.DataContext = viewModel.tokens;
                }
            }
            else
            {
                var message = new MessageDialog(loader.GetString("MainPage_FirstContact_Text"), loader.GetString("MainPage_FirstContact_Title"));
                await message.ShowAsync();
                Frame.Navigate(typeof(Pages.Settings.AuthorizePage));
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                if (e.NavigationMode != NavigationMode.Back)
                {
                    if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                    {
                        Init();
                    }
                    else
                    {
                        var message = new MessageDialog(loader.GetString("NetworkErrorMessage"), loader.GetString("ErrorTitle1"));
                        Task.Run(() => message.ShowAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                var message = new MessageDialog(ex.Message, loader.GetString("ErrorTitle1"));
                Task.Run(() => message.ShowAsync());//TODO: バグレポートを送れるようにする。（サーバー建てるか？）
            }

            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += _BackRequested;
            base.OnNavigatedTo(e);
        }

        private void _BackRequested(object sender, BackRequestedEventArgs e)
        {
        }

        private async Task ShowMessage(Exception ex)
        {
            var message = new MessageDialog(ex.Message + "\n" + ex.InnerException.Message, "おや？ なにかがおかしいようです。");
            await message.ShowAsync();
        }

        private void SettingsButtonTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(Pages.Settings.SettingsPage));
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
            var ReplyTweet = (AdvancedTweet)(((Button)sender).DataContext);
            TweetTextBox.ReplyTweet = ReplyTweet;
            var replyTo = ReplyTweet.tweet.user.screen_name;
            replyTo = "@" + replyTo + " ";
            TweetTextBox.TweetTextBox.Text = replyTo + TweetTextBox.TweetTextBox.Text;

            var items = TweetUserList.Items;
            var select = from item in items
                         where ((UserAccessToken)item).screenName == ReplyTweet.accessToken.screenName
                         select item as UserAccessToken;

            TweetUserList.SelectedItem = select.First();
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

        private void NewTweetButtonClicked(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(Pages.CreateTweetPage));
        }

        private void SearchButtonClicked(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(Pages.SearchPage));
        }

        /// <summary>
        /// 広告を閉じる
        /// </summary>
        private void CloseAd(object sender, RoutedEventArgs e)
        {
            var ad = viewModel.columns.Last();
            viewModel.columns.Remove(ad);
        }

        private void TweetButton_Click(object sender, RoutedEventArgs e)
        {
            TweetTextBox.TweetButtonClicked(sender, e);
        }

        private void QuoteTweet(object sender, RoutedEventArgs e)
        {
            AdvancedTweet tweet = (AdvancedTweet)((Button)sender).DataContext;
            var url = "https://twitter.com/statuses/" + tweet.tweet.id_str;
            TweetTextBox.TweetTextBox.Text = TweetTextBox.TweetTextBox.Text + url;
        }
    }
}

