using Kurosuke_Universal.ViewModels;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using Kurosuke_Universal.Utils;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace Kurosuke_Universal.Pages
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class UserDetail : Page
    {
        UserAccessToken data;
        AdvancedUser viewModel;
        public UserDetail()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += _BackRequested;
            data = (UserAccessToken)e.Parameter;
            await data.Init();
            var imageUrl = "https://twitter.com/" + data.screenName + "/profile_image?size=original";
            data.user.profile_image_url_https = imageUrl;
            viewModel = new AdvancedUser(data);
            mainGrid.DataContext = viewModel;
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

        private void UserNameTapped(object sender, TappedRoutedEventArgs e)
        {
            var uri = new Uri("https://twitter.com/" + data.screenName);
            Helpers.OpenBrowser(Frame, uri);
        }

        private void GoToTweetlistPage(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(TweetListPage), data);
        }

        private void FollowersCliced(object sender, RoutedEventArgs e)
        {
            var data = new UserListPage.UserListPageDataTransfer(UserListPage.UserListMode.Followed, this.data);
            Frame.Navigate(typeof(UserListPage), data);
        }

        private void FriendsClicked(object sender, RoutedEventArgs e)
        {
            var data = new UserListPage.UserListPageDataTransfer(UserListPage.UserListMode.Following, this.data);
            Frame.Navigate(typeof(UserListPage), data);
        }
    }
}
