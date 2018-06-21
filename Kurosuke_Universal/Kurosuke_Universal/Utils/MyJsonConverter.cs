using System.Collections.Generic;
using Newtonsoft.Json;
using Kurosuke_Universal.Models;
using Kurosuke_Universal.ViewModels;
using System;
using System.Globalization;
using Windows.UI.Xaml.Media;
using System.Text.RegularExpressions;
using System.Net;

namespace Kurosuke_Universal.Utils
{
    public static class MyJsonConverter
    {
        public static AdvancedTweet DeserializeStreamTweet(string json, AccessToken accessToken)
        {
            var jsonObject = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(json);
            if (jsonObject != null)
            {
                var exists = jsonObject.Property("event");

                if (exists == null)
                {
                    var isFriends = jsonObject.Property("friends");
                    if (isFriends == null)
                    {
                        exists = jsonObject.Property("delete");
                        if (exists == null)
                        {
                            var tweet = JsonConvert.DeserializeObject<Tweet>(json);
                            return FormatTweet(new AdvancedTweet(tweet, accessToken));
                        }
                        else
                        {
                            var delete = JsonConvert.DeserializeObject<DeleteJsonObject>(json);
                            return new AdvancedTweet(delete, accessToken);
                        }
                    }
                    else
                    {
                        var friends = JsonConvert.DeserializeObject<Friends>(json);
                        return new AdvancedTweet(friends, accessToken);
                    }
                }
                else
                {
                    var @event = JsonConvert.DeserializeObject<Event>(json);
                    return new AdvancedTweet(@event, accessToken);
                }
            }
            return null;
        }

        public static List<AdvancedTweet> DeserializeTweets(string json, AccessToken accessToken)
        {
            var tweets = JsonConvert.DeserializeObject<List<Tweet>>(json);
            var newList = AdvancedTweet.ConvertTweets(tweets, accessToken);
            List<AdvancedTweet> returnList = new List<AdvancedTweet>();
            foreach (AdvancedTweet tweet in newList)
            {
                returnList.Add(FormatTweet(tweet));
            }
            return returnList;
        }

        public static List<AdvancedTweet> DeserializeSearchResult(string json, AccessToken accessToken)
        {
            var result = JsonConvert.DeserializeObject<SearchResult>(json);
            var tweets = result.statuses;
            var newList = AdvancedTweet.ConvertTweets(tweets, accessToken);
            List<AdvancedTweet> returnList = new List<AdvancedTweet>();
            foreach (AdvancedTweet tweet in newList)
            {
                returnList.Add(FormatTweet(tweet));
            }
            return returnList;
        }

        private static AdvancedTweet FormatTweet(AdvancedTweet tweet)
        {
            //まずはTweetがRetweetなのか、などの振り分け。
            if (tweet.tweet.retweeted_status != null)
            {
                tweet.message = "@" + tweet.tweet.user.screen_name + " retweeted";
                tweet.source = tweet.tweet;
                tweet.tweet = tweet.tweet.retweeted_status;
                tweet.state = AdvancedTweet.States.Retweeted;
                tweet.subImage = "Assets/TwitterIcons/retweet.png";
            }
            else
            {
                tweet.state = AdvancedTweet.States.Tweet;
            }



            if (tweet.tweet.entities.urls != null)
            {
                foreach (Url url in tweet.tweet.entities.urls)
                {
                    tweet.tweet.text = tweet.tweet.text.Replace(url.url, url.display_url);
                }
            }
            if (tweet.tweet.entities.media != null)
            {
                foreach (Media url in tweet.tweet.entities.media)
                {
                    tweet.tweet.text = tweet.tweet.text.Replace(url.url, url.display_url);
                }
            }

            //mentionに自分がいたらツイートの色を変える
            foreach (var mention in tweet.tweet.entities.user_mentions)
            {
                if (mention.screen_name == tweet.accessToken.screenName)
                {
                    tweet.ButtonColor = new SolidColorBrush(Windows.UI.Colors.Azure);
                }
            }

            //textのフォントサイズ指定（できれば毎回StoreSettingsを呼び出したくないぞ...）
            var settings = new StoreSettings();
            var index = settings.TryGetValueWithDefault("TextFontSize", 1);
            switch (index)
            {
                case 0:
                    tweet.tweet.text_font_size = 17;
                    break;
                case 1:
                    tweet.tweet.text_font_size = 12;
                    break;
                case 2:
                    tweet.tweet.text_font_size = 7;
                    break;
                default:
                    break;
            }

            Regex rx = new Regex("<a href=\"(?<url>.*?)\".*?>(?<text>.*?)</a>");
            var mc = rx.Match(tweet.tweet.source);
            if (mc.Success)
            {
                tweet.tweet.source = mc.Groups["text"].Value;
            }
            tweet.createdAtDatetime = DateTime.ParseExact(tweet.tweet.created_at, "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture);
            tweet.tweet.created_at_time = tweet.createdAtDatetime.ToString("HH:mm");
            tweet.tweet.created_at_datetime = tweet.createdAtDatetime.ToString("yyyy/MM/dd HH:mm:ss");

            tweet.tweet.text = WebUtility.HtmlDecode(tweet.tweet.text);
            return tweet;
        }
    }
}
