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
    public sealed partial class EasyWebViewerPage : Page
    {
        ObservableCollection<WebPageObject> webPageCollection;
        public EasyWebViewerPage()
        {
            this.InitializeComponent();
            webPageCollection = new ObservableCollection<WebPageObject>();
            WebViewFlipView.DataContext = webPageCollection;
        }

        private void _BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            var isNotEmpty = WebViewFlipView.BackRequested();
            if (!isNotEmpty)
            {
                if (Frame.CanGoBack)
                {
                    webPageCollection.Clear();
                    Frame.GoBack();
                }
            }
        }
        private void BackButtonClicked(object sender, RoutedEventArgs e)
        {
            webPageCollection.Clear();
            Frame.GoBack();
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += _BackRequested;

            var uri = (Uri)e.Parameter;
            var flipView = WebViewFlipView.flipView;
            WebPageObject page = new WebPageObject(webPageCollection, uri, flipView);
            webPageCollection.Add(page);
        }
    }
}
