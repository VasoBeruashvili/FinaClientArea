using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinaClientArea.Models
{
    public class NewsModel
    {
        public int ID { get; set; }

        [JsonProperty("ti")]
        public string Title { get; set; }

        [JsonProperty("td")]
        public DateTime TDate { get; set; }

        [JsonProperty("mtn")]
        public string ModuleTypeName { get; set; }

        [JsonProperty("tn")]
        public string TypeName { get; set; }

        [JsonProperty("bt")]
        public string BodyText { get; set; }
    }
}