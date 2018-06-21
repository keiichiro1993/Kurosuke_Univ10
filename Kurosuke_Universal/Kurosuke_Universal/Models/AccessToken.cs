namespace Kurosuke_Universal.Models
{
    public class AccessToken
    {
        public string token { get; set; }
        public string tokenSecret { get; set; }
        public string screenName { get; set; }

        public AccessToken(string accessToken, string accessTokenSecret)
        {
            token = accessToken;
            tokenSecret = accessTokenSecret;
        }
        public AccessToken(string accessToken, string accessTokenSecret, string screenName)
        {
            token = accessToken;
            tokenSecret = accessTokenSecret;
            this.screenName = screenName;
        }

        public AccessToken()
        {
        }
    }
}
