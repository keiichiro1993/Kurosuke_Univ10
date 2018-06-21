using Kurosuke_Universal.Utils;
using Kurosuke_Universal.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Kurosuke_Universal.CustomControl
{
    public sealed partial class CustomWebView : UserControl
    {
        public WebPageObject WebPage;
        public WebView InnerWebView { get { return webView; } }
        public CustomWebView()
        {
            this.InitializeComponent();
            this.webView.LoadCompleted += webView_LoadCompleted;
            this.webView.NewWindowRequested += WebView_NewWindowRequested;
        }

        private void WebView_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            args.Handled = true;

            if (!(WebPage.Collection.Count - 1 == WebPage.Index))
            {
                for (int i = WebPage.Collection.Count - 1; i > WebPage.Index; i--)
                {
                    WebPage.Collection.RemoveAt(i);
                }
            }
            var page = new WebPageObject(WebPage.Collection, args.Uri, WebPage.flipView);
            WebPage.Collection.Add(page);
            WebPage.flipView.SelectedItem = page;
        }

        public WebPageObject Source
        {
            get { return (WebPageObject)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // 依存関係プロパティ
        public static readonly DependencyProperty SourceProperty =
          DependencyProperty.Register(
            "Source", typeof(WebPageObject), typeof(CustomWebView),
            new PropertyMetadata(null, new PropertyChangedCallback(OnSourceChanged))
          );

        // 依存関係プロパティに値がセットされたときに呼び出されるメソッド
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as CustomWebView;
            thisInstance.WebPage = e.NewValue as WebPageObject;
            thisInstance.StartLoading(thisInstance.WebPage.uri);
        }

        private void StartLoading(Uri newUri)
        {
            // webViewを隠し、progressを動かす
            //this.webView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.progress.IsActive = true;

            // webViewにWebページへのアクセスを開始させる
            this.webView.Navigate(newUri);
        }

        // webViewでロードが完了したときのイベント・ハンドラ
        void webView_LoadCompleted(object sender, NavigationEventArgs e)
        {
            // webViewを表示し、progressを止める
            //this.webView.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.progress.IsActive = false;
            this.webView.NavigationStarting += WebView_NavigationStarting;
        }

        private void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (args.Uri == this.webView.BaseUri)
            {
                TmpUserData.PreviousNavigationDateTime = DateTime.Now;
            }
            else
            {
                args.Cancel = true;
                if (TmpUserData.PreviousNavigationDateTime == null)
                {
                    TmpUserData.PreviousNavigationDateTime = DateTime.Now;
                }
                else
                {
                    if (DateTime.Now - TmpUserData.PreviousNavigationDateTime < new TimeSpan(0, 0, 2))
                    {
                        return;
                    }
                }
                TmpUserData.PreviousNavigationDateTime = DateTime.Now;
                if (!(WebPage.Collection.Count - 1 == WebPage.Index))
                {
                    for (int i = WebPage.Collection.Count - 1; i > WebPage.Index; i--)
                    {
                        WebPage.Collection.RemoveAt(i);
                    }
                }

                var page = new WebPageObject(WebPage.Collection, args.Uri, WebPage.flipView);
                this.WebPage.Collection.Add(page);
                WebPage.flipView.SelectedItem = page;
            }
        }
    }
}
