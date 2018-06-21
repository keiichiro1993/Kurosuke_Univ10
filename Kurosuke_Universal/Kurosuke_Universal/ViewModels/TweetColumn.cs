using Kurosuke_Universal.Models;
using Kurosuke_Universal.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Kurosuke_Universal.ViewModels
{
    public class TweetColumn
    {
        public enum ColumnRoles { HomeTimeLine, Mention, DirectMessage, Search, UserTimeLine };//これによって呼び出すメンバ関数を変えれば、一つのTweetColumnクラスですべてのカラムに対応できそう。
        public ObservableCollection<AdvancedTweet> tweetList { get; set; }
        public AccessToken accessToken { get; set; }
        public ColumnRoles columnRole;
        public string columnTitle { get; set; }

        public TweetColumn(AccessToken accessToken, ColumnRoles columnRole)
        {
            this.accessToken = accessToken;
            this.columnRole = columnRole;
            tweetList = new ObservableCollection<AdvancedTweet>();
            columnTitle = columnRole.ToString();
        }

        //UserTimeLine用コンストラクタ
        private string userScreenName;
        public TweetColumn(AccessToken accessToken, string userScreenName)
        {
            this.accessToken = accessToken;
            columnRole = ColumnRoles.UserTimeLine;
            tweetList = new ObservableCollection<AdvancedTweet>();
            columnTitle = "Tweets of @" + userScreenName;
            this.userScreenName = userScreenName;
        }

        public async Task Run()
        {
            var setting = new StoreSettings();
            var count = setting.TryGetValueWithDefault("TweetCount", 50);
            switch (columnRole)
            {
                case ColumnRoles.HomeTimeLine:
                    await GetHomeTimeline(count, null);
                    break;

                case ColumnRoles.Mention:
                    await GetMentionsTimeline(count, null);
                    break;

                case ColumnRoles.DirectMessage:
                    break;

                case ColumnRoles.Search:
                    break;

                case ColumnRoles.UserTimeLine:
                    await GetUserTimeline(count, null);
                    break;

                default:
                    throw new Exception("columnRollが指定されていません。");
            }
        }

        /*ホームタイムラインを取得するメソッド*/
        public async Task GetHomeTimeline(int count, AdvancedTweet oldTweet)
        {
            TwitterClient client = new TwitterClient(accessToken);
            var list = await client.GetTimeline(count, oldTweet, "https://api.twitter.com/1.1/statuses/home_timeline.json");
            InsertTweet(list);
            InsertLastButton();
        }

        public async void GetAdditionalTimeline(int count)
        {
            tweetList.RemoveAt(tweetList.Count - 1);
            await GetHomeTimeline(count, this.tweetList[tweetList.Count - 1]);
        }

        public async Task GetMentionsTimeline(int count, AdvancedTweet lastTweet)
        {
            TwitterClient client = new TwitterClient(accessToken);
            var list = await client.GetTimeline(count, lastTweet, "https://api.twitter.com/1.1/statuses/mentions_timeline.json");
            InsertTweet(list);
        }

        public string lastWord;
        public async Task GetSearchResultTimeline(string word, int count, AdvancedTweet lastTweet)
        {
            lastWord = word;
            TwitterClient client = new TwitterClient(accessToken);
            var list = await client.GetSearchResult(count, lastTweet, word);
            InsertTweet(list);
            InsertLastButton();
        }
        public async void GetAdditionalSearchResult(int count)
        {
            tweetList.RemoveAt(tweetList.Count - 1);
            var last = this.tweetList[tweetList.Count - 1];
            await GetSearchResultTimeline(lastWord, count, last);
        }

        public async Task GetUserTimeline(int count, AdvancedTweet oldTweet)
        {
            TwitterClient client = new TwitterClient(accessToken);
            var list = await client.GetUserTimeline(count, userScreenName, oldTweet);
            InsertTweet(list);
            InsertLastButton();
        }

        public async void GetAdditionalUserTimeline(int count)
        {
            tweetList.RemoveAt(tweetList.Count - 1);
            var last = this.tweetList[tweetList.Count - 1];
            await GetUserTimeline(count, last);
        }

        private void InsertLastButton()
        {
            var button = new AdvancedTweet(this);
            button.tweetRole = AdvancedTweet.TweetRoles.LoadButton;
            tweetList.Add(button);
        }

        /*ツイートをリストに順番に追加する*/
        private void InsertTweet(List<AdvancedTweet> list)
        {
            //list.Reverse();//逆順に
            foreach (var tweet in list)
            {
                tweetList.Insert(tweetList.Count, tweet);
            }
        }
    }
}
