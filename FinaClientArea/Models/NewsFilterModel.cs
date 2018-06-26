using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinaClientArea.Models
{
    public class NewsFilterModel
    {
        public string k { get; set; }
        public string c { get; set; }
        public int li { get; set; }
        public List<int?> mti { get; set; }
    }
}