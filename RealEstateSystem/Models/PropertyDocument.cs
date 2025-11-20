using System;
using System.ComponentModel.DataAnnotations;

namespace RealEstateSystem.Models
{
    public class PropertyDocument
    {
        [Key]
        public int DocumentId { get; set; }

        public int PropertyId { get; set; }
        public Property Property { get; set; }

        [MaxLength(150)]
        public string DocumentName { get; set; }

        [MaxLength(250)]
        public string DocumentUrl { get; set; }

        public DocumentType DocumentType { get; set; }

        public DateTime UploadedDate { get; set; } = DateTime.Now;
    }
}
