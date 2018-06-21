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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Kurosuke_Universal.CustomControl
{
    public sealed partial class WebFlipView : UserControl
    {
        public WebFlipView()
        {
            this.InitializeComponent();
            flipViewInstance.SelectionChanged += FlipView_SelectionChanged;
        }

        public FlipView flipView
        {
            get { return flipViewInstance; }
        }

        public TextBox urlBlock
        {
            get { return urlTextBlock; }
        }

        public ObservableCollection<WebPageObject> ItemsSource
        {
            get { return (ObservableCollection<WebPageObject>)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // 依存関係プロパティ
        public static readonly DependencyProperty SourceProperty =
          DependencyProperty.Register(
            "ItemsSource", typeof(ObservableCollection<WebPageObject>), typeof(WebFlipView),
            new PropertyMetadata(null, new PropertyChangedCallback(OnSourceChanged))
          );

        // 依存関係プロパティに値がセットされたときに呼び出されるメソッド
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisInstance = d as WebFlipView;
            thisInstance.flipViewInstance.DataContext = e.NewValue as ObservableCollection<WebPageObject>;
        }

        private async static void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await Task.Delay(100);
            var flip = (FlipView)sender;
            if (flip != null)
            {
                var item = (WebPageObject)flip.SelectedItem;
                var grid = (Grid)flip.Parent;
                if (grid != null)
                {
                    var instance = (WebFlipView)grid.Parent;
                    instance.urlTextBlock.Text = item.uri.OriginalString;
                }
            }
        }

        private void BackButtonClicked(object sender, RoutedEventArgs e)
        {
            if (flipViewInstance.SelectedIndex > 0)
            {
                flipViewInstance.SelectedIndex--;
            }
        }

        public bool BackRequested()
        {
            if (flipViewInstance.SelectedIndex > 0)
            {
                flipViewInstance.SelectedIndex--;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ForwardButtonClicked(object sender, RoutedEventArgs e)
        {
            if (flipViewInstance.SelectedIndex < flipViewInstance.Items.Count - 1)
            {
                flipViewInstance.SelectedIndex++;
            }
        }

        private void RefreshButtonClicked(object sender, RoutedEventArgs e)
        {
            WebPageObject item = (WebPageObject)flipViewInstance.SelectedItem;
            var container = flipViewInstance.ContainerFromItem(item);
            var children = AllChildren(container);

            var view = (CustomWebView)children.First(x => x.Name == "WebViewContainer");
            view.InnerWebView.Refresh();
        }

        public List<Control> AllChildren(DependencyObject parent)
        {
            var _List = new List<Control>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var _Child = VisualTreeHelper.GetChild(parent, i);
                if (_Child is Control)
                {
                    _List.Add(_Child as Control);
                }
                _List.AddRange(AllChildren(_Child));
            }
            return _List;
        }

        private void urlTextBlock_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var item = (WebPageObject)flipViewInstance.Items.First();
                var newPage = new WebPageObject(item.Collection, new Uri(urlTextBlock.Text), flipViewInstance);
                item.Collection.Add(newPage);
                flipViewInstance.SelectedItem = newPage;
            }
        }

        private async void OpenInBrowserButtonClicked(object sender, RoutedEventArgs e)
        {
            var page = (WebPageObject)flipViewInstance.SelectedItem;
            bool success = await Windows.System.Launcher.LaunchUriAsync(page.uri);
            if (!success)
            {
                var message = new MessageDialog("ブラウザの起動に失敗しました。", "おや？ なにかがおかしいようです。");
                await message.ShowAsync();
            }
        }
    }
}
