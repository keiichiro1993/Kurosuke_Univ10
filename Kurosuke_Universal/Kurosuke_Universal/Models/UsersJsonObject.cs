using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kurosuke_Universal.Models
{
    public class FollowersListJsonObject
    {
        public List<User> users { get; set; }
        public long next_cursor { get; set; }
        public string next_cursor_str { get; set; }
        public int previous_cursor { get; set; }
        public string previous_cursor_str { get; set; }
    }

    public class UserIdListJsonObject
    {
        public List<long> ids { get; set; }
        public long next_cursor { get; set; }
        public string next_cursor_str { get; set; }
        public int previous_cursor { get; set; }
        public string previous_cursor_str { get; set; }
    }
}
