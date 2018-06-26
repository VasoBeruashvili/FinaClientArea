using System;

namespace FinaClientArea.Models
{
    public class CRMIncidentModel
    {
        public int ID { get; set; }

        public string GeneralDoc_Purpose { get; set; }
        public DateTime? GeneralDoc_Tdate { get; set; }
        public int? GeneralDoc_StatusID { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string CRMIncidentType_Name { get; set; }
    }
}