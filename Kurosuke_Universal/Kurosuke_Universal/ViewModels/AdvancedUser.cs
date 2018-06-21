using Kurosuke_Universal.Models;
using Kurosuke_Universal.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;
using Windows.Web.Http;

namespace Kurosuke_Universal.ViewModels
{
    public class AdvancedUser : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private UserAccessToken userAccessToken;
        private int followingState;
        public User user { get; set; }
        public Relationship relationship { get; set; }
        public string keyImageUrl { get; set; }
        public string buttonText { get; set; }
        public SolidColorBrush buttonColor { get; set; }//ツイートのバックグラウンド色を選択


        public AdvancedUser(UserAccessToken userAccessToken)
        {
            this.userAccessToken = userAccessToken;
            Init();
        }
        public async void Init()
        {
            user = userAccessToken.user;
            if (user.@protected == true)
            {
                keyImageUrl = "/Assets/Icons/lock.png";
            }

            if ((bool)user.following)
            {
                buttonText = "Following(Tap to Unfollow)";
                buttonColor = new SolidColorBrush(Windows.UI.Colors.LightBlue);
                followingState = 0;
            }
            else
            {
                if ((bool)user.follow_request_sent)
                {
                    buttonText = "Requesting";
                    buttonColor = new SolidColorBrush(Windows.UI.Colors.Coral);
                    followingState = 2;
                }
                else
                {
                    buttonText = "Follow";
                    buttonColor = new SolidColorBrush(Windows.UI.Colors.Azure);
                    followingState = 1;
                }
            }
            try
            {
                var client = new TwitterClient(userAccessToken.accessToken);
                var relationshipObj = await client.GetRelationship(user.id_str);
                relationship = relationshipObj.relationship;
                OnPropertyChanged("relationship");
            }
            catch (Exception ex)
            {
                var message = new MessageDialog(ex.Message, "おや？何かがおかしいようです。");
                await message.ShowAsync();
            }
        }

        private ICommand _followCommand;

        public ICommand FollowCommand
        {
            get
            {
                if (_followCommand == null) _followCommand = new DelegateCommand<object>(FollowFunction);
                return _followCommand;
            }
        }

        private async void FollowFunction(object obj)
        {
            var client = new TwitterClient(userAccessToken.accessToken);
            try
            {
                switch (followingState)
                {
                    case 0:
                        await client.UnFollow(user);
                        await userAccessToken.Init();
                        user = userAccessToken.user;
                        if ((bool)user.follow_request_sent)
                        {
                            buttonText = "Requesting";
                            buttonColor = new SolidColorBrush(Windows.UI.Colors.Coral);
                            followingState = 2;
                        }
                        else if (!(bool)user.following)
                        {
                            followingState = 1;
                            buttonText = "Follow";
                            buttonColor = new SolidColorBrush(Windows.UI.Colors.Azure);
                        }
                        break;
                    case 1:
                        await client.Follow(user);
                        await userAccessToken.Init();
                        user = userAccessToken.user;
                        if ((bool)user.following)
                        {
                            buttonText = "Following(Tap to Unfollow)";
                            buttonColor = new SolidColorBrush(Windows.UI.Colors.LightBlue);
                            followingState = 0;
                        }
                        break;
                    case 2:
                        break;
                    default:
                        break;
                }
                UpdateFollowingState();
            }
            catch (Exception ex)
            {
                var message = new MessageDialog(ex.Message, "おや？ なにかがおかしいようです。");
                await message.ShowAsync();
            }
        }

        private void UpdateFollowingState()
        {
            OnPropertyChanged("buttonText");
            OnPropertyChanged("buttonColor");
        }
    }
}
