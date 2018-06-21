using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kurosuke_Universal.Models
{
    public class Target
    {
        public string id_str { get; set; }
        public long id { get; set; }
        public bool followed_by { get; set; }
        public string screen_name { get; set; }
        public bool following { get; set; }
    }

    public class Source
    {
        public bool can_dm { get; set; }
        public bool blocking { get; set; }
        public string id_str { get; set; }
        public bool all_replies { get; set; }
        public bool want_retweets { get; set; }
        public long id { get; set; }
        public bool marked_spam { get; set; }
        public bool followed_by { get; set; }
        public bool notifications_enabled { get; set; }
        public string screen_name { get; set; }
        public bool following { get; set; }
    }

    public class Relationship
    {
        public Target target { get; set; }
        public Source source { get; set; }
    }

    public class RelationshipJsonObject
    {
        public Relationship relationship { get; set; }
    }
}
