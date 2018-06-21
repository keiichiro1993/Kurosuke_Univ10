using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace Kurosuke_Universal.Utils
{
    public static class Helpers
    {
        public static async void OpenBrowser(Frame frame, Uri uri)
        {
            var setting = new StoreSettings();
            var usingInAppBrowser = setting.TryGetValueWithDefault("UseInAppBrowser", true);
            if (usingInAppBrowser)
            {
                frame.Navigate(typeof(Pages.EasyWebViewerPage), uri);
            }
            else
            {
                bool success = await Windows.System.Launcher.LaunchUriAsync(uri);
                if (!success)
                {
                    var message = new MessageDialog("ブラウザの起動に失敗しました。", "おや？ なにかがおかしいようです。");
                    await message.ShowAsync();
                }
            }
        }
    }
}
