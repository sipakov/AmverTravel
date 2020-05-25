using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Amver.Domain.Models
{
    [JsonObject]
    public class Push
    {
        public List<string> registration_ids { get; set; }
        
        public Notification notification { get; set; }

        public Data data { get; set; }
    }

    public class Notification
    {
        public string body { get; set; }

        public string title { get; set; }

        public bool content_available { get; set; }

        public string priority { get; set; }

        public int badge { get; set; }

        public string sound { get; set; }

        public string icon { get; set; }
    }
}
