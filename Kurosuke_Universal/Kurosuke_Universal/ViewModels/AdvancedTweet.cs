using Kurosuke_Universal.Models;
using Kurosuke_Universal.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Web.Http;

namespace Kurosuke_Universal.ViewModels
{
    public class AdvancedTweet : INotifyPropertyChanged //Tweetにメソッドを追加していくクラス
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        public AccessToken accessToken { get; set; }

        public enum TweetRoles { Tweet, Event, Friends, Delete, LoadButton }

        public TweetRoles tweetRole { get; set; }
        public Tweet tweet { get; set; }
        public Event @event { get; set; }
        public Friends friends { get; set; }
        public DeleteJsonObject delete { get; set; }

        public DateTime createdAtDatetime { get; set; }

        /*リツイートの表示やふぁぼられの表示に利用*/
        public string message { get; set; }
        public string subImage { get; set; }
        public States state { get; set; }
        public Tweet source { get; set; }
        public enum States { Tweet, Retweeted, Favorited }

        public SolidColorBrush ButtonColor { get; set; }//ツイートのバックグラウンド色を選択

        public AdvancedTweet(AccessToken accessToken)
        {
            this.accessToken = accessToken;
            ButtonColor = new SolidColorBrush(Windows.UI.Colors.White);
        }

        public AdvancedTweet(Tweet tweet, AccessToken accessToken) : this(accessToken)
        {
            this.tweet = tweet;
            this.tweetRole = TweetRoles.Tweet;
            Init();
        }

        public AdvancedTweet(Event eventItem, AccessToken accessToken) : this(accessToken)
        {
            this.@event = eventItem;
            this.tweetRole = TweetRoles.Event;
        }

        public AdvancedTweet(Friends friends, AccessToken accessToken) : this(accessToken)
        {
            this.friends = friends;
            this.tweetRole = TweetRoles.Friends;
        }

        public AdvancedTweet(DeleteJsonObject delete, AccessToken accessToken) : this(accessToken)
        {
            this.delete = delete;
            this.tweetRole = TweetRoles.Delete;
        }


        //さらに読み込むボタン専用のコンストラクタ
        private TweetColumn column;
        public AdvancedTweet(TweetColumn column)
        {
            this.column = column;
        }

        private void Init()
        {
            if ((bool)tweet.favorited)
            {
                favoriteImageUrl = "/Assets/TwitterIcons/favorite_on.png";
            }
            else
            {
                favoriteImageUrl = "/Assets/TwitterIcons/favorite.png";
            }

            if ((bool)tweet.retweeted)
            {
                retweetedImageUrl = "/Assets/TwitterIcons/retweet_on.png";
            }
            else
            {
                retweetedImageUrl = "/Assets/TwitterIcons/retweet.png";
            }

            replyImageUrl = "/Assets/TwitterIcons/reply.png";
        }

        public static List<AdvancedTweet> ConvertTweets(List<Tweet> tweets, AccessToken accessToken)
        {
            List<AdvancedTweet> newList = new List<AdvancedTweet>();
            foreach (Tweet tweet in tweets)
            {
                newList.Add(new AdvancedTweet(tweet, accessToken));
            }
            return newList;
        }


        /*これ以降、ボタンの画像をBindingする*/
        private string _favoriteImageUrl;
        public string favoriteImageUrl
        {
            get { return _favoriteImageUrl; }
            set
            {
                _favoriteImageUrl = value;
                OnPropertyChanged("favoriteImageUrl");
            }
        }

        private string _retweetedImageUrl;
        public string retweetedImageUrl
        {
            get { return _retweetedImageUrl; }
            set
            {
                _retweetedImageUrl = value;
                OnPropertyChanged("retweetedImageUrl");
            }
        }
        public string replyImageUrl { get; set; }
        public string detailImageUrl { get; set; }

        /*これ以降、favやretweetを実装し、relaycommandでxamlと結びつける（propertyChangedを実装する必要があるかも）*/

        private ICommand _favoriteCommand;

        public ICommand FavoriteCommand
        {
            get
            {
                if (_favoriteCommand == null) _favoriteCommand = new DelegateCommand<object>(Favorite);
                return _favoriteCommand;
            }
        }

        private async void Favorite(object obj)
        {
            var client = new TwitterClient(accessToken);
            HttpResponseMessage res;
            try
            {
                if (state == States.Retweeted)
                {
                    res = await client.ChangeFavorite(source);
                }
                else
                {
                    res = await client.ChangeFavorite(tweet);
                }

                if (!res.IsSuccessStatusCode)
                {
                    throw new Exception(res.ReasonPhrase);
                }
                else
                {
                    if ((bool)tweet.favorited)
                    {
                        favoriteImageUrl = "/Assets/TwitterIcons/favorite.png";
                        tweet.favorited = false;
                    }
                    else
                    {
                        favoriteImageUrl = "/Assets/TwitterIcons/favorite_on.png";
                        tweet.favorited = true;
                    }
                }
            }
            catch (Exception ex)
            {
                //throw new Exception("おや？　なにかがおかしいようです。", ex);
                var message = new MessageDialog(ex.Message, "おや？ なにかがおかしいようです。");
                await message.ShowAsync();
            }
        }


        /*複数ユーザーからのリツイートを実現するため、リツイートはメインページから。*/
        private ICommand _retweetCommand;

        public ICommand RetweetCommand
        {
            get
            {
                if (_retweetCommand == null) _retweetCommand = new DelegateCommand<object>(Retweet);
                return _retweetCommand;
            }
        }

        private async void Retweet(object obj)
        {
            if ((bool)tweet.retweeted)
            {
                var message = new MessageDialog("（震え声）", "リツイートの解除は甘え");
                await message.ShowAsync();
            }
            else
            {
                var client = new TwitterClient(accessToken);
                HttpResponseMessage res;
                try
                {
                    res = await client.ChangeRetweet(tweet);
                    if (!res.IsSuccessStatusCode)
                    {
                        throw new Exception(res.ReasonPhrase);
                    }
                    else
                    {
                        if ((bool)tweet.retweeted)
                        {
                            retweetedImageUrl = "/Assets/TwitterIcons/retweet.png";//TODO: Retweetを削除できるようにする（Retweetによって生成されたTweetを削除する必要がある。）
                            tweet.retweeted = false;
                        }
                        else
                        {
                            retweetedImageUrl = "/Assets/TwitterIcons/retweet_on.png";
                            tweet.retweeted = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //throw new Exception("おや？　なにかがおかしいようです。", ex);
                    var message = new MessageDialog(ex.Message, "おや？ なにかがおかしいようです。");
                    await message.ShowAsync();
                }
            }
        }

        private ICommand _loadCommand;

        public ICommand LoadCommand
        {
            get
            {
                if (_loadCommand == null) _loadCommand = new DelegateCommand<object>(Load);
                return _loadCommand;
            }
        }

        private void Load(object obj)
        {
            StoreSettings setting = new StoreSettings();
            var count = setting.TryGetValueWithDefault("TweetCount", 50);

            switch (column.columnRole)
            {
                case TweetColumn.ColumnRoles.HomeTimeLine:
                    column.GetAdditionalTimeline(count);
                    break;

                case TweetColumn.ColumnRoles.Search:
                    column.GetAdditionalSearchResult(count);
                    break;

                case TweetColumn.ColumnRoles.UserTimeLine:
                    column.GetAdditionalUserTimeline(count);
                    break;

                default:
                    throw new Exception("このカラムでは追加のツイートを取得できません。");
            }
        }
    }
}
