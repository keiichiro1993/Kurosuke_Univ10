using Kurosuke_Universal.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Kurosuke_Universal.ViewModels
{
    public class UserAccessToken : INotifyPropertyChanged
    {
        public AccessToken accessToken { get; set; }
        public string screenName;
        private long id;
        private User _user;
        public User user
        {
            get { return _user; }
            set
            {
                this._user = value;
                OnPropertyChanged("user");
            }
        }

        public UserAccessToken(AccessToken accessToken)
        {
            this.accessToken = accessToken;
            screenName = accessToken.screenName;
            Init();
        }

        public UserAccessToken(AccessToken accessToken, string screenName)
        {
            this.accessToken = accessToken;
            this.screenName = screenName;
        }

        public UserAccessToken(AccessToken accessToken, long id)
        {
            this.accessToken = accessToken;
            this.id = id;
            Init();
        }

        public UserAccessToken(AccessToken accessToken, User user)
        {
            this.accessToken = accessToken;
            screenName = user.screen_name;
            this.user = user;
        }

        public UserAccessToken(User user)
        {
            this.user = user;
        }

        public async Task Init()
        {

            var client = new TwitterClient(accessToken);
            try
            {
                if (string.IsNullOrEmpty(screenName))
                {
                    user = await client.GetUserDetailById(id);
                    screenName = user.screen_name;
                }
                else
                {
                    user = await client.GetUserDetailByScreenName(screenName);
                }
            }
            catch (Exception ex)
            {
                var message = new MessageDialog("ユーザーの取得中にエラーが発生しました。\n" + ex.Message + ":" + ex.InnerException.Message, "おや？ なにかがおかしいようです。");
                await message.ShowAsync();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

    }
}
