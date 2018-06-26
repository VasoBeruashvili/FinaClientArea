using Newtonsoft.Json;
using System;

namespace FinaClientArea.Models
{
    public class AccountLicenseResponse
    {
        public int id { get; set; }
        public Guid key { get; set; }
        [JsonProperty("package")]
        public int PackageID { get; set; }
        public DateTime deadline { get; set; }
        public string PackageName { get; set; }
        public string tdate { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }
    }
}