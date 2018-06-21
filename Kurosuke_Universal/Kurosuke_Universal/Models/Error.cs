using System.Collections.Generic;

namespace Kurosuke_Universal.Models
{
    public class Error
    {
        public int code { get; set; }
        public string message { get; set; }
    }

    public class Errors
    {
        public List<Error> errors { get; set; }
    }
}
