using System;
using System.Collections.Generic;
using System.Text;

namespace IceCreamRatingAPI
{
    public class UserRating
    {

        public string id { get; set; }
        public string userId { get; set; }
        public string productId { get; set; }
        public DateTime timestamp { get; set; }
        public string locationName { get; set; }
        public int rating { get; set; }
        public string userNotes { get; set; }
    }
}
