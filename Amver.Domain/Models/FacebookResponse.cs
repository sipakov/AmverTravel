using System;

namespace Amver.Domain.Models
{
    public class FacebookResponse
    {
        public string id { get; set; }
        public Picture picture { get; set; }
        public string first_name { get; set; }
        public string email { get; set; }
        public string last_name { get; set; }
        public string gender { get; set; }
        public DateTime birthday { get; set; }
    }
    public class DataFb
    {
        public int height { get; set; }
        public bool is_silhouette { get; set; }
        public string url { get; set; }
        public int width { get; set; }
    }
    public class Picture
    {
        public DataFb data { get; set; }
    }
}