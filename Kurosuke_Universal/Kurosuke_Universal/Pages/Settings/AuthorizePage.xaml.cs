using Kurosuke_Universal.Clients;
using Kurosuke_Universal.Models;
using Kurosuke_Universal.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Kurosuke_Universal.Pages.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AuthorizePage : Page
    {
        private OAuthorizer authorizer;
        private StoreSettings storage;
        private ObservableCollection<AccessToken> tokens;
        private ResourceLoader loader;
        public AuthorizePage()
        {
            this.InitializeComponent();
            loader = new ResourceLoader();
            authorizer = new OAuthorizer();
            storage = new StoreSettings();
            tokens = storage.TryGetValueWithDefault<ObservableCollection<AccessToken>>("AccessTokens", null);
            if (tokens == null)
            {
                tokens = new ObservableCollection<AccessToken>();
            }
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += AuthorizePage_BackRequested;
        }

        private void AuthorizePage_BackRequested(object sender, BackRequestedEventArgs e)
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
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            var url = await authorizer.GetPinEnterUrl();
            AuthWebView.Navigate(new Uri(url));
            base.OnNavigatedTo(e);
        }

        private async void AuthButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            string pin = PINBox.Text;
            if (!String.IsNullOrEmpty(pin))
            {
                var accessToken = await authorizer.GetAccessToken(pin);
                var exist = from token in tokens
                            where token.screenName == accessToken.screenName
                            select token;

                if (exist.Count() != 0)
                {
                    var message = new MessageDialog(loader.GetString("Settings_AuthorizePage_AccountDuplicated"), loader.GetString("ErrorTitle1"));
                    await message.ShowAsync();
                }
                else
                {
                    if (accessToken.token != null && accessToken.tokenSecret != null && accessToken.screenName != null)
                    {
                        tokens.Add(accessToken);
                        storage.AddOrUpdateValue("AccessTokens", tokens);
                        var message = new MessageDialog(loader.GetString("Settings_AuthorizePage_AccountAdded"));
                        await message.ShowAsync();
                        Frame.Navigate(typeof(MainPage));
                    }
                    else
                    {

                    }
                }
            }
            else
            {
                var message = new MessageDialog(loader.GetString("Settings_AuthorizePage_EnterThePin"), loader.GetString("ErrorTitle1"));
                await message.ShowAsync();
            }
        }

        private void BackButtonClicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
