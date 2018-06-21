using Kurosuke_Universal.Models;
using OAuth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace Kurosuke_Universal.Clients
{
    public class OAuthClient
    {
        private static string consumerKey = MyAccount.consumer;
        private static string consumerSecret = MyAccount.consumerSecret;
        private static readonly string AUTHHEADER_BASE = "OAuth oauth_consumer_key =\"{0}\", oauth_nonce=\"{1}\", oauth_signature=\"{2}\",oauth_signature_method=\"{3}\", oauth_timestamp=\"{4}\", oauth_token=\"{5}\", oauth_version=\"{6}\"";
        private AccessToken accessToken;
        private OAuthorizer authorizer;
        public CancellationTokenSource streamCancel;//ユーザーストリームのキャンセルに使う

        public OAuthClient(AccessToken accessToken)
        {
            this.accessToken = accessToken;
            authorizer = new OAuthorizer();
        }


        /*OAuthするためのHeaderを作る*/
        public string GenerateAuthHeader(string requestUrl, string requestMethod)
        {
            OAuthBase oauthBase = new OAuthBase();
            string timestamp = oauthBase.GenerateTimeStamp();
            string nonce = oauthBase.GenerateNonce();

            string normalizedUrl, normalizedReqParams;
            normalizedUrl = normalizedReqParams = string.Empty;

            Uri uri = new Uri(requestUrl);

            string signature = oauthBase.GenerateSignature(
                uri,
                consumerKey,
                consumerSecret,
                accessToken.token,
                accessToken.tokenSecret,
                requestMethod,
                timestamp,
                nonce,
                OAuthBase.SignatureTypes.HMACSHA1,
                out normalizedUrl,
                out normalizedReqParams);
            signature = WebUtility.UrlEncode(signature);

            string authHeader = string.Format(
                AUTHHEADER_BASE,
                consumerKey,
                nonce,
                signature,
                "HMAC-SHA1",
                timestamp,
                accessToken.token,
                "1.0"
                );

            return authHeader;
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            HttpClient client = new HttpClient();
            var authHeader = GenerateAuthHeader(url, "GET");
            HttpResponseMessage result = new HttpResponseMessage();
            try
            {
                client.DefaultRequestHeaders.Add("Authorization", authHeader);
                result = await client.GetAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public async Task<HttpResponseMessage> PostAsync(string url, IHttpContent content)
        {
            HttpClient client = new HttpClient();
            var authHeader = GenerateAuthHeader(url, "POST");
            HttpResponseMessage result = new HttpResponseMessage();
            try
            {
                client.DefaultRequestHeaders.Add(new KeyValuePair<string, string>("ContentType", "application/x-www-form-urlencoded"));
                client.DefaultRequestHeaders.Add("Authorization", authHeader);
                result = await client.PostAsync(new Uri(url), content);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public async Task<HttpResponseMessage> PostWithMediaAsync(string url, IHttpContent content)
        {
            HttpClient client = new HttpClient();
            var authHeader = GenerateAuthHeader(url, "POST");
            HttpResponseMessage result = new HttpResponseMessage();
            try
            {
                
                client.DefaultRequestHeaders.Add("Authorization", authHeader);
                result = await client.PostAsync(new Uri(url), content);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public async Task GetStreamAsync(string url, Action<string> fetchAction)
        {
            HttpClient client = new HttpClient();
            var authHeader = GenerateAuthHeader(url, "GET");
            client.DefaultRequestHeaders.Add("Authorization", authHeader);

            streamCancel = new CancellationTokenSource(new TimeSpan(0, 30, 0));
            try
            {
                var res = await client.GetAsync(new Uri(url), HttpCompletionOption.ResponseHeadersRead).AsTask(streamCancel.Token);
                using (var stream = (await res.Content.ReadAsInputStreamAsync()).AsStreamForRead())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        while (!await Task.Run(() => reader.EndOfStream) && !streamCancel.IsCancellationRequested)
                        {
                            var s = await reader.ReadLineAsync();
                            if (!String.IsNullOrEmpty(s))
                            {
                                fetchAction(s);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ストリーミングが継続できないようです。:" + ex.Message);
            }
        }
    }
}
