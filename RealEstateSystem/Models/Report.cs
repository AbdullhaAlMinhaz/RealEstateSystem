using System;
using System.ComponentModel.DataAnnotations;

namespace RealEstateSystem.Models
{
    public class Report
    {
        [Key]
        public int ReportId { get; set; }

        public ReportType ReportType { get; set; }

        [MaxLength(200)]
        public string ReportTitle { get; set; }

        public int GeneratedBy { get; set; }
        public User GeneratedByUser { get; set; }

        public DateTime GeneratedDate { get; set; } = DateTime.Now;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string ReportData { get; set; }

        [MaxLength(250)]
        public string ReportFileUrl { get; set; }
    }
}
