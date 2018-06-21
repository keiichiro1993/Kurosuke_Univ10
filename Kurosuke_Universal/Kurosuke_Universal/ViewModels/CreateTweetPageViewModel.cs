using Kurosuke_Universal.Models;
using Kurosuke_Universal.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Kurosuke_Universal.ViewModels
{
    public class CreateTweetPageViewModel
    {
        public ObservableCollection<SendableImage> Images { get; set; }
        public ObservableCollection<UserAccessToken> Tokens { get; set; }
        public CreateTweetPageViewModel()
        {
            Init();
        }

        public async void Init()
        {
            Images = new ObservableCollection<SendableImage>();

            var storage = new StoreSettings();
            var tokens = storage.TryGetValueWithDefault<ObservableCollection<AccessToken>>("AccessTokens", null);
            if (tokens != null)
            {
                if (tokens.Count != 0)
                {
                    Tokens = TmpUserData.Accounts;
                }
            }
            else
            {
                var message = new MessageDialog("アカウントがありません。まずは設定からアカウントを追加してください。", "おや？なにかがおかしいようです。");
                await message.ShowAsync();
            }
        }
    }
}
