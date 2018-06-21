
using Kurosuke_Universal.Models;
using System.Collections.Generic;
using Kurosuke_Universal.ViewModels;
using Kurosuke_Universal.Clients;
using Windows.Web.Http;
using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Kurosuke_Universal.Utils;
using System.Collections.ObjectModel;
using Windows.UI.Popups;
using System.Linq;

namespace Kurosuke_Universal
{
    public class TwitterClient
    {
        private AccessToken accessToken;

        public TwitterClient(AccessToken accessToken)
        {
            this.accessToken = accessToken;
        }

        /// <summary>
        /// タイムラインを取得するメソッド
        /// </summary>
        /// <param name="count">取得するツイート数</param>
        /// <param name="lastTweet">続きを取得するときに利用（nullでよい）</param>
        /// <param name="requestUrl">リクエストURL（home_timelineなのか、mention_timelineなのか...）</param>
        /// <returns></returns>
        public async Task<List<AdvancedTweet>> GetTimeline(int count, AdvancedTweet oldTweet, string requestUrl)
        {
            CreateRequestUrl(count, oldTweet, ref requestUrl);
            return await GetTimelineByUrl(requestUrl);
        }

        private void CreateRequestUrl(int count, AdvancedTweet oldTweet, ref string requestUrl)
        {
            requestUrl += "?count=" + count.ToString();
            if (oldTweet != null)
            {
                if (oldTweet.state == AdvancedTweet.States.Retweeted)
                {
                    oldTweet = new AdvancedTweet(oldTweet.source, accessToken);
                }
                requestUrl += "&max_id=" + oldTweet.tweet.id_str;
            }
        }

        public async Task<List<AdvancedTweet>> GetUserTimeline(int count, string screenName, AdvancedTweet oldTweet)
        {
            var requestUrl = "https://api.twitter.com/1.1/statuses/user_timeline.json";
            CreateRequestUrl(count, oldTweet, ref requestUrl);
            requestUrl += "&screen_name=" + screenName;
            return await GetTimelineByUrl(requestUrl);
        }

        private async Task<List<AdvancedTweet>> GetTimelineByUrl(string requestUrl)
        {
            OAuthClient client = new OAuthClient(accessToken);
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                response = await client.GetAsync(requestUrl);
            }
            catch (Exception ex)
            {
                throw new Exception("ネットワークエラーでタイムラインを取得できません", ex);
            }

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return MyJsonConverter.DeserializeTweets(json, accessToken);
            }
            else
            {
                throw new Exception("レスポンスメッセージのStatusCodeがSuccessではありません。");
            }
        }

        public async Task<ObservableCollection<UserAccessToken>> GetMyFollowers(string requestUrl)
        {
            OAuthClient client = new OAuthClient(accessToken);
            HttpResponseMessage response = new HttpResponseMessage();
            ObservableCollection<UserAccessToken> userAccessTokens = new ObservableCollection<UserAccessToken>();
            try
            {
                response = await client.GetAsync(requestUrl);
            }
            catch (Exception ex)
            {
                throw new Exception("ネットワークエラーでユーザー一覧を取得できません", ex);
            }

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<FollowersListJsonObject>(json);
                var returnUsers = new ObservableCollection<UserAccessToken>();
                foreach (var user in users.users)
                {
                    returnUsers.Add(new UserAccessToken(accessToken, user));
                }
                return returnUsers;
            }
            else
            {
                throw new Exception("レスポンスメッセージのStatusCodeがSuccessではありません。");
            }
        }
        public async Task<ObservableCollection<UserAccessToken>> GetUserListById(string requestUrl)
        {
            OAuthClient client = new OAuthClient(accessToken);
            HttpResponseMessage response = new HttpResponseMessage();
            ObservableCollection<UserAccessToken> userAccessTokens = new ObservableCollection<UserAccessToken>();
            try
            {
                response = await client.GetAsync(requestUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var users = JsonConvert.DeserializeObject<UserIdListJsonObject>(json);
                    List<User> newusers;
                    newusers = await this.GetUsersDetailsByIds(users.ids);
                    var returnUsers = new ObservableCollection<UserAccessToken>();
                    foreach (var user in newusers)
                    {
                        returnUsers.Add(new UserAccessToken(accessToken, user));
                    }
                    return returnUsers;
                }
                else
                {
                    throw new Exception("レスポンスメッセージのStatusCodeがSuccessではありません。");
                }
            }
            catch (Exception ex)
            {
                var message = new MessageDialog(ex.Message, "ユーザー一覧を取得できません");
                await message.ShowAsync();
            }
            return null;
        }

        public async Task<ObservableCollection<UserAccessToken>> GetUserListFromTweets(string requestUrl)
        {
            OAuthClient client = new OAuthClient(accessToken);
            HttpResponseMessage response = new HttpResponseMessage();
            ObservableCollection<UserAccessToken> userAccessTokens = new ObservableCollection<UserAccessToken>();
            try
            {
                response = await client.GetAsync(requestUrl);
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(ex.Message, "ユーザー一覧を取得できません");
                await dialog.ShowAsync();
            }

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                List<AdvancedTweet> tweets = MyJsonConverter.DeserializeTweets(json, accessToken);
                foreach (var tweet in tweets)
                {
                    var item = new UserAccessToken(accessToken, tweet.source.user);
                    userAccessTokens.Add(item);
                }
                return userAccessTokens;
            }
            else
            {
                var dialog = new MessageDialog(response.ReasonPhrase, "レスポンスメッセージのStatusCodeがSuccessではありません。");
                await dialog.ShowAsync();
            }
            return null;
        }

        public async Task GetStream(Action<AdvancedTweet> fetchAction)
        {
            OAuthClient client = new OAuthClient(accessToken);
            await client.GetStreamAsync("https://userstream.twitter.com/1.1/user.json", (json) =>
            {
                AdvancedTweet advancedTweet;
                try
                {
                    advancedTweet = MyJsonConverter.DeserializeStreamTweet(json, accessToken);
                }
                catch (Exception ex)
                {
                    throw new Exception("また知らない形のjsonが投げられたみたい。:" + json, ex);
                }
                fetchAction(advancedTweet);
            });
        }

        public async Task UpdateStatus(string status, AdvancedTweet replyToTweet)
        {
            OAuthClient client = new OAuthClient(accessToken);

            try
            {
                using (var content = new HttpFormUrlEncodedContent(new[] { new KeyValuePair<string, string>("status", status) }))
                {
                    var postAddress = "https://api.twitter.com/1.1/statuses/update.json?status=";
                    postAddress += Uri.EscapeUriString(status);
                    if (replyToTweet != null)
                    {
                        postAddress += "&in_reply_to_status_id=" + replyToTweet.tweet.id;
                    }
                    var res = await client.PostAsync(postAddress, null);//TODO: なぜHttpFormUrlEncodedContentが使えないのか調べる
                    if (!res.IsSuccessStatusCode)
                    {
                        var errors = JsonConvert.DeserializeObject<Errors>(res.Content.ToString());
                        throw new Exception("ErrorCode: " + errors.errors[0].code + "\n" + errors.errors[0].message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(accessToken.screenName + "でツイートできませんでした。", ex);
            }

        }

        public async Task<List<AdvancedTweet>> GetSearchResult(int count, AdvancedTweet oldTweet, string word)
        {
            var requestUrl = "https://api.twitter.com/1.1/search/tweets.json";
            word = Uri.EscapeUriString(word);
            requestUrl += "?q=" + word;
            requestUrl += "&count=" + count.ToString();
            if (oldTweet != null)
            {
                if (oldTweet.state == AdvancedTweet.States.Retweeted)
                {
                    oldTweet = new AdvancedTweet(oldTweet.source, accessToken);
                }
                requestUrl += "&max_id=" + oldTweet.tweet.id_str;
            }
            OAuthClient client = new OAuthClient(accessToken);
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                response = await client.GetAsync(requestUrl);
            }
            catch (Exception ex)
            {
                throw new Exception("ネットワークエラーでタイムラインを取得できません", ex);
            }

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return MyJsonConverter.DeserializeSearchResult(json, accessToken);
            }
            else
            {
                throw new Exception("レスポンスメッセージのStatusCodeがSuccessではありません。");
            }
        }

        public async Task UpdateStatusWithMedia(HttpMultipartFormDataContent content)
        {
            OAuthClient client = new OAuthClient(accessToken);

            try
            {
                var postAddress = "https://api.twitter.com/1.1/statuses/update_with_media.json";

                var res = await client.PostWithMediaAsync(postAddress, content);
                if (!res.IsSuccessStatusCode)
                {
                    var errors = JsonConvert.DeserializeObject<Errors>(res.Content.ToString());
                    throw new Exception("ErrorCode: " + errors.errors[0].code + "\n" + errors.errors[0].message);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(accessToken.screenName + "でツイートできませんでした。", ex);
            }
        }

        public async Task<HttpResponseMessage> ChangeFavorite(Tweet tweet)
        {
            OAuthClient client = new OAuthClient(accessToken);
            string change;
            if ((bool)tweet.favorited)
            {
                change = "destroy";
            }
            else
            {
                change = "create";
            }

            var res = await client.PostAsync("https://api.twitter.com/1.1/favorites/" + change + ".json?id=" + tweet.id_str, null);//TODO: なぜHttpFormUrlEncodedContentが使えないのか調べる
            return ErrorCheck(res);
        }

        public async Task<HttpResponseMessage> ChangeRetweet(Tweet tweet)
        {
            OAuthClient client = new OAuthClient(accessToken);
            var res = await client.PostAsync("https://api.twitter.com/1.1/statuses/retweet/" + tweet.id_str + ".json", null);
            return ErrorCheck(res);
        }

        public async Task<HttpResponseMessage> Follow(User user)
        {
            OAuthClient client = new OAuthClient(accessToken);
            var res = await client.PostAsync("https://api.twitter.com/1.1/friendships/create.json?user_id=" + user.id_str + "&follow=true", null);
            return ErrorCheck(res);
        }

        public async Task<HttpResponseMessage> UnFollow(User user)
        {
            OAuthClient client = new OAuthClient(accessToken);
            var res = await client.PostAsync("https://api.twitter.com/1.1/friendships/destroy.json?user_id=" + user.id_str, null);
            return ErrorCheck(res);
        }

        public async Task<RelationshipJsonObject> GetRelationship(string id)
        {
            OAuthClient client = new OAuthClient(accessToken);
            var res = await client.PostAsync("https://api.twitter.com/1.1/friendships/update.json?user_id=" + id, null);
            ErrorCheck(res);
            return JsonConvert.DeserializeObject<RelationshipJsonObject>(res.Content.ToString());
        }

        public async Task<User> GetUserDetailByScreenName(string screenName)
        {
            var uri = "https://api.twitter.com/1.1/users/show.json?screen_name=" + screenName;
            User user = await GetUserDetailByUriGet<User>(uri);
            return user;
        }

        public async Task<User> GetUserDetailById(long id)
        {
            var uri = "https://api.twitter.com/1.1/users/show.json?user_id=" + id;
            User user = await GetUserDetailByUriGet<User>(uri);
            return user;
        }

        public async Task<List<User>> GetUsersDetailsByIds(List<long> ids)
        {
            List<User> returnUser = new List<User>();

            for (var j = 0; j < ids.Count / 100 + 1; j++)
            {
                var tmpIds = ids.Skip(100 * j).Take(100).ToList();
                var uri = "https://api.twitter.com/1.1/users/lookup.json?user_id=";
                string idsstring = ids[0].ToString();
                if (tmpIds.Count > 1)
                {
                    for (var i = 1; i < tmpIds.Count; i++)
                    {
                        idsstring += "%2C" + tmpIds[i].ToString();
                    }
                }
                uri += idsstring;
                List<User> users = await GetUserDetailByUriPost<List<User>>(uri);
                returnUser.AddRange(users);
            }
            return returnUser;
        }

        private async Task<Type> GetUserDetailByUriGet<Type>(string uri)
        {
            var client = new OAuthClient(accessToken);
            Type user;
            try
            {
                var res = await client.GetAsync(uri);
                user = JsonConvert.DeserializeObject<Type>(res.Content.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("ユーザーの取得に失敗しました。", ex);
            }

            return user;
        }
        private async Task<Type> GetUserDetailByUriPost<Type>(string uri)
        {
            var client = new OAuthClient(accessToken);
            Type user;
            try
            {
                var res = await client.PostAsync(uri, null);
                if (!res.IsSuccessStatusCode)
                {
                    throw new Exception(res.ReasonPhrase);
                }
                user = JsonConvert.DeserializeObject<Type>(res.Content.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("ユーザーの取得に失敗しました。", ex);
            }

            return user;
        }
        public async Task<AdvancedTweet> GetTweet(string id)
        {
            var client = new OAuthClient(accessToken);
            AdvancedTweet tweet;
            HttpResponseMessage res = new HttpResponseMessage();
            try
            {
                res = await client.GetAsync("https://api.twitter.com/1.1/statuses/show.json?id=" + id);
                tweet = MyJsonConverter.DeserializeStreamTweet(res.Content.ToString(), accessToken);
                return tweet;
            }
            catch (Exception ex)
            {
                try
                {

                    var errors = JsonConvert.DeserializeObject<Errors>(res.Content.ToString());
                    var error = errors.errors[0];
                    throw new Exception("Error " + error.code + ":" + error.message);
                }
                catch (Exception)
                {
                    throw new Exception("ツイートの取得に失敗しました。", ex);
                }
            }
        }

        private static HttpResponseMessage ErrorCheck(HttpResponseMessage res)
        {
            if (!res.IsSuccessStatusCode)
            {
                var errors = JsonConvert.DeserializeObject<Errors>(res.Content.ToString());
                throw new Exception("ErrorCode: " + errors.errors[0].code + "\n" + errors.errors[0].message);
            }
            else
            {
                return res;
            }
        }
    }
}
