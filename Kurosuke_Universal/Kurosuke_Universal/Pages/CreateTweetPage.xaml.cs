using Kurosuke_Universal.Models;
using Kurosuke_Universal.Utils;
using Kurosuke_Universal.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace Kurosuke_Universal.Pages
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class CreateTweetPage : Page
    {
        public CreateTweetPageViewModel viewModel;
        ResourceLoader loader;
        CreateTweetPageTransfer TransferData;
        //画像付きツイートなどを行うのはこのページから。
        //メインページのツイート欄は速攻で打ちたい時用にしよう。（モバイルを意識するとこうなるよね）
        public CreateTweetPage()
        {
            this.InitializeComponent();
            loader = new ResourceLoader();
            this.Loaded += CreateTweetPage_Loaded;
        }

        private void CreateTweetPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (TransferData != null)
            {
                switch (TransferData.role)
                {
                    case CreateTweetPageTransferRole.Reply:
                        TweetTextBox.ReplyTweet = TransferData.tweet;
                        TweetTextBox.TweetTextBox.Text += "@" + TransferData.tweet.tweet.user.screen_name + " ";
                        var items = TweetUserList.Items;
                        var select = from item in items
                                     where ((UserAccessToken)item).screenName == TransferData.tweet.accessToken.screenName
                                     select item as UserAccessToken;
                        TweetUserList.SelectedItem = select.First();
                        break;
                    case CreateTweetPageTransferRole.QuotTweet:
                        var url = "https://twitter.com/statuses/" + TransferData.tweet.tweet.id_str;
                        TweetTextBox.TweetTextBox.Text = TweetTextBox.TweetTextBox.Text + url;
                        break;
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            viewModel = new CreateTweetPageViewModel();
            MainGrid.DataContext = viewModel;

            var data = (CreateTweetPageTransfer)e.Parameter;
            TransferData = data;


            TweetTextBox.TweetButton = TweetButton;
            TweetTextBox.TweetUserList = TweetUserList;
            TweetTextBox.Images = viewModel.Images;
            TweetTextBox.CurrentFrame = this.Frame;
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += CreateTweetPage_BackRequested;
        }

        private void CreateTweetPage_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
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

        private async void AddImageClicked(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.SettingsIdentifier = loader.GetString("CreateTweetPage_FilePickerTitle");

            var files = await picker.PickMultipleFilesAsync();

            foreach (var file in files)
            {
                if (viewModel.Images.Count < 4)
                {
                    try
                    {
                        viewModel.Images.Add(new SendableImage(file));
                    }
                    catch (Exception ex)
                    {
                        var message = new MessageDialog(ex.Message, loader.GetString("ErrorTitle1"));
                    }
                }
                else
                {
                    var message = new MessageDialog(loader.GetString("CreateTweetPage_FilePickerError"), loader.GetString("ErrorTitle1"));
                }
            }
        }

        private async void ImageItemsClicked(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var item = (SendableImage)button.DataContext;

            var message = new MessageDialog(item.file.Name + loader.GetString("CreateTweetPage_FileDeleteDialog_Title"));
            message.Commands.Add(new UICommand(loader.GetString("CreateTweetPage_FileDeleteDialog_Yes")));
            message.Commands.Add(new UICommand(loader.GetString("CreateTweetPage_FileDeleteDialog_No")));
            message.DefaultCommandIndex = 1;

            var command = await message.ShowAsync();
            if (command.Label == loader.GetString("CreateTweetPage_FileDeleteDialog_Yes"))
            {
                viewModel.Images.Remove(item);
            }
        }

        private void TweetButton_Click(object sender, RoutedEventArgs e)
        {
            TweetTextBox.TweetButtonClicked(sender, e);
        }


        /*ページ遷移用*/
        public enum CreateTweetPageTransferRole { Reply, QuotTweet }

        public class CreateTweetPageTransfer
        {
            public CreateTweetPageTransferRole role;
            public AdvancedTweet tweet;

            public CreateTweetPageTransfer(CreateTweetPageTransferRole role, AdvancedTweet tweet)
            {
                this.role = role;
                this.tweet = tweet;
            }
        }
    }
}
