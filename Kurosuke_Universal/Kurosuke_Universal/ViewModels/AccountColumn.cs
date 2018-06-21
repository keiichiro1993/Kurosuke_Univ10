using Kurosuke_Universal.Models;
using Kurosuke_Universal.Utils;
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
    public class AccountColumn
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public enum AccountColumnRole { AccountColumn, Advertising };

        public AccountColumnRole accountColumnRole { get; set; }

        public AccessToken accessToken { get; set; }
        private ObservableCollection<UserAccessToken> tokens;
        public ObservableCollection<TweetColumn> tweetColumns { get; set; }
        public string columnTitle { get; set; }
        private int tweetCountUpper;

        private bool _IsStreamRunning;
        public bool IsStreamRunning
        {
            get { return _IsStreamRunning; }
            set
            {
                this._IsStreamRunning = value;
                OnPropertyChanged("IsStreamRunning");
            }
        }

        private string _IsStreamRunningString;
        public string IsStreamRunningString
        {
            get { return _IsStreamRunningString; }
            set
            {
                this._IsStreamRunningString = value;
                OnPropertyChanged("IsStreamRunningString");
            }
        }

        public AccountColumn()
        {
            accountColumnRole = AccountColumnRole.Advertising;
        }
        public AccountColumn(AccessToken accessToken, ObservableCollection<UserAccessToken> tokens)
        {
            this.accessToken = accessToken;
            tweetColumns = new ObservableCollection<TweetColumn>();
            columnTitle = "@" + accessToken.screenName;
            this.tokens = tokens;

            var setting = new StoreSettings();
            tweetCountUpper = setting.TryGetValueWithDefault("TweetCount", 50);
            accountColumnRole = AccountColumnRole.AccountColumn;
        }

        public async Task GetStream(int retryCount)
        {
            TwitterClient client = new TwitterClient(accessToken);
            IsStreamRunning = true;
            IsStreamRunningString = "Streaming...";
            try
            {
                await client.GetStream(async (tweet) =>
                {
                    if (tweet != null)
                    {
                        switch (tweet.tweetRole)//ここでいろいろ振り分けるぞ～^^
                        {
                            case AdvancedTweet.TweetRoles.Tweet:
                                tweetColumns[0].tweetList.Insert(0, tweet);
                                var user = tweet.tweet.entities.user_mentions.Where(x => x.screen_name == accessToken.screenName).FirstOrDefault();
                                if (user != null)
                                {
                                    tweetColumns[1].tweetList.Insert(0, tweet);
                                }
                                break;

                            case AdvancedTweet.TweetRoles.Event:
                                break;

                            case AdvancedTweet.TweetRoles.Delete:
                                break;

                            case AdvancedTweet.TweetRoles.Friends:
                                var storage = new StoreSettings();
                                var cache = new StoreCache();
                                List<User> friends;

                                if (TmpUserData.Friends == null)
                                {
                                    TmpUserData.Friends = new ObservableCollection<User>();
                                }

                                var prev = storage.TryGetValueWithDefault("FriendCount" + accessToken.screenName, 0);

                                if (prev == tweet.friends.friends.Count)
                                {
                                    friends = await cache.TryLoadCache<List<User>>("FriendObject" + accessToken.screenName);
                                    if (friends == null)
                                    {
                                        var friendsClient = new TwitterClient(accessToken);
                                        friends = await friendsClient.GetUsersDetailsByIds(tweet.friends.friends);
                                        storage.AddOrUpdateValue("FriendCount" + accessToken.screenName, tweet.friends.friends.Count);
                                        await cache.SaveCache("FriendObject" + accessToken.screenName, friends);
                                    }
                                }
                                else
                                {
                                    var friendsClient = new TwitterClient(accessToken);
                                    friends = await friendsClient.GetUsersDetailsByIds(tweet.friends.friends);
                                    storage.AddOrUpdateValue("FriendCount" + accessToken.screenName, tweet.friends.friends.Count);
                                    await cache.SaveCache("FriendObject" + accessToken.screenName, friends);
                                }

                                if (friends != null)
                                {
                                    foreach (var friend in friends)
                                    {
                                        TmpUserData.Friends.Add(friend);
                                    }
                                }
                                break;

                            default:
                                break;
                        }
                        foreach (var column in tweetColumns)
                        {
                            var count = column.tweetList.Count;
                            if (count > tweetCountUpper)
                            {
                                column.tweetList.RemoveAt(count - 2);
                                column.tweetList.RemoveAt(count - 3);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                if (retryCount < 3)
                {
                    var message = new MessageDialog("ユーザーストリームへの接続に失敗しました。リトライします。" + ex.Message, "おや？なにかがおかしいようです。");
                    await message.ShowAsync();
                    await GetStream(++retryCount);
                }
                else
                {
                    IsStreamRunning = false;
                    IsStreamRunningString = "Disconnected.";
                    var message = new MessageDialog("ユーザーストリームへの接続に3回連続で失敗しました。ネットワークへの接続を確認してください。:" + ex.Message, "おや？なにかがおかしいようです。");
                    await message.ShowAsync();
                }
            }
        }

        public async void Run()
        {
            tweetColumns.Add(new TweetColumn(accessToken, TweetColumn.ColumnRoles.HomeTimeLine));
            tweetColumns.Add(new TweetColumn(accessToken, TweetColumn.ColumnRoles.Mention));
            foreach (TweetColumn column in tweetColumns)
            {
                try
                {
                    await column.Run();
                }
                catch (Exception ex)
                {
                    var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                    var message = new MessageDialog(loader.GetString("ErrorTitle1"), loader.GetString("NetworkErrorMessageBody") + ex.Message);
                }
            }

            await GetStream(0);
        }
    }
}
