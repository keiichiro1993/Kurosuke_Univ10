using System;
using System.Collections.Generic;

namespace Kurosuke_Universal.Models
{
    public class Coordinates
    {
        public List<double> coordinates { get; set; }
        public string type { get; set; }
    }
    public class Thumb
    {
        public int w { get; set; }
        public int h { get; set; }
        public string resize { get; set; }
    }

    public class Small
    {
        public int w { get; set; }
        public int h { get; set; }
        public string resize { get; set; }
    }

    public class Medium
    {
        public int w { get; set; }
        public int h { get; set; }
        public string resize { get; set; }
    }

    public class Large
    {
        public int w { get; set; }
        public int h { get; set; }
        public string resize { get; set; }
    }

    public class Sizes
    {
        public Thumb thumb { get; set; }
        public Small small { get; set; }
        public Medium medium { get; set; }
        public Large large { get; set; }
    }

    public class Media
    {
        public long? id { get; set; }
        public string id_str { get; set; }
        public List<int> indices { get; set; }
        public string media_url { get; set; }
        public string media_url_https { get; set; }
        public string url { get; set; }
        public string display_url { get; set; }
        public string expanded_url { get; set; }
        public string type { get; set; }
        public Sizes sizes { get; set; }
        public long? source_status_id { get; set; }
        public string source_status_id_str { get; set; }
        public long? source_user_id { get; set; }
        public string source_user_id_str { get; set; }
    }

    public class UserMention
    {
        public string screen_name { get; set; }
        public string name { get; set; }
        public long? id { get; set; }
        public string id_str { get; set; }
        public List<int> indices { get; set; }
    }

    public class Hashtag
    {
        public string text { get; set; }
        public List<int> indices { get; set; }
    }

    public class Entities
    {
        public List<Url> urls { get; set; }
        public List<Hashtag> hashtags { get; set; }
        public List<object> trends { get; set; }
        public List<UserMention> user_mentions { get; set; }
        public List<object> symbols { get; set; }
        public List<Media> media { get; set; }
    }

    public class ExtendedEntities
    {
        public List<Media> media { get; set; }
    }


    public class Geo
    {
        public List<double> coordinates { get; set; }
        public string type { get; set; }
    }

    public class Attributes
    {
    }

    public class BoundingBox
    {
        public List<List<List<double>>> coordinates { get; set; }
        public string type { get; set; }
    }

    public class Place
    {
        public string name { get; set; }
        public string country_code { get; set; }
        public string country { get; set; }
        public Attributes attributes { get; set; }
        public string url { get; set; }
        public string id { get; set; }
        public BoundingBox bounding_box { get; set; }
        public string full_name { get; set; }
        public string place_type { get; set; }
    }

    public class Url
    {
        public string expanded_url { get; set; }
        public string url { get; set; }
        public List<int> indices { get; set; }
        public string display_url { get; set; }
    }

    public class Urls
    {
        public List<Url> urls { get; set; }
    }

    public class Description
    {
        public List<object> urls { get; set; }
    }

    public class Entities2
    {
        public Urls url { get; set; }
        public Description description { get; set; }
    }

    public class User
    {
        public string name { get; set; }
        public string profile_sidebar_fill_color { get; set; }
        public bool? profile_background_tile { get; set; }
        public string profile_sidebar_border_color { get; set; }
        public string profile_image_url { get; set; }
        public string created_at { get; set; }
        public string location { get; set; }
        public bool? follow_request_sent { get; set; }
        public string id_str { get; set; }
        public bool? is_translator { get; set; }
        public string profile_link_color { get; set; }
        public Entities2 entities { get; set; }
        public bool? default_profile { get; set; }
        public string url { get; set; }
        public bool? contributors_enabled { get; set; }
        public int favourites_count { get; set; }
        public int? utc_offset { get; set; }
        public string profile_image_url_https { get; set; }
        public long? id { get; set; }
        public int listed_count { get; set; }
        public bool? profile_use_background_image { get; set; }
        public string profile_text_color { get; set; }
        public int followers_count { get; set; }
        public string lang { get; set; }
        public bool? @protected { get; set; }
        public bool? geo_enabled { get; set; }
        public bool? notifications { get; set; }
        public string description { get; set; }
        public string profile_background_color { get; set; }
        public bool? verified { get; set; }
        public string time_zone { get; set; }
        public string profile_background_image_url_https { get; set; }
        public int statuses_count { get; set; }
        public string profile_background_image_url { get; set; }
        public bool? default_profile_image { get; set; }
        public int friends_count { get; set; }
        public bool? following { get; set; }
        public bool? show_all_inline_media { get; set; }
        public string screen_name { get; set; }
    }

    public class Tweet
    {
        public Coordinates coordinates { get; set; }
        public bool? truncated { get; set; }
        public string created_at { get; set; }
        /*datetimeを利用して見やすい文字列に(以下追加)*/
        public string created_at_time { get; set; }
        public string created_at_datetime { get; set; }

        public bool? favorited { get; set; }
        public string id_str { get; set; }
        public string in_reply_to_user_id_str { get; set; }
        public Entities entities { get; set; }
        public ExtendedEntities extended_entities { get; set; }
        public string text { get; set; }
        //textのフォントサイズ指定のために必要なんや許してや...
        public int text_font_size { get; set; } = 12;
        public object contributors { get; set; }
        public long? id { get; set; }
        public int retweet_count { get; set; }
        public int favorite_count { get; set; }
        public string in_reply_to_status_id_str { get; set; }
        public Geo geo { get; set; }
        public bool? retweeted { get; set; }
        public Tweet retweeted_status { get; set; }
        public long? in_reply_to_user_id { get; set; }
        public Place place { get; set; }
        public string source { get; set; }
        public User user { get; set; }
        public string in_reply_to_screen_name { get; set; }
        public long? in_reply_to_status_id { get; set; }
        public bool? possibly_sensitive { get; set; }
        public string lang { get; set; }
    }

    public class Event
    {
        public string @event { get; set; }
        public string created_at { get; set; }
        public User source { get; set; }
        public User target { get; set; }
        public Tweet target_object { get; set; }
    }

    public class Friends
    {
        public List<long> friends { get; set; }
    }

    public class Status
    {
        public long id { get; set; }
        public string id_str { get; set; }
        public int user_id { get; set; }
        public string user_id_str { get; set; }
    }

    public class Delete
    {
        public Status status { get; set; }
        public string timestamp_ms { get; set; }
    }

    public class DeleteJsonObject
    {
        public Delete delete { get; set; }
    }
}
