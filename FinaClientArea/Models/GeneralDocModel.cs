using System;

namespace FinaClientArea.Models
{
    public class GeneralDocModel
    {
        public int ID { get; set; }
        public string Purpose { get; set; }
        public DateTime? TDate { get; set; }
        public double? Amount { get; set; }
    }
}