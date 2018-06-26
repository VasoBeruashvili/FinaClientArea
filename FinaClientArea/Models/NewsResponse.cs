using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinaClientArea.Models
{
    public class NewsResponse
    {
        [JsonProperty("li")]
        public int LastID { get; set; }

        [JsonProperty("em")]
        public string ErrorMessage { get; set; }

        [JsonProperty("nm")]
        public List<NewsModel> NewsItems { get; set; }
    }
}