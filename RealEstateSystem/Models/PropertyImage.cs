using System;
using System.ComponentModel.DataAnnotations;

namespace RealEstateSystem.Models
{
    public class PropertyImage
    {
        [Key]
        public int ImageId { get; set; }

        public int PropertyId { get; set; }
        public Property Property { get; set; }

        [Required, MaxLength(250)]
        public string ImageUrl { get; set; }

        public bool IsPrimary { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime UploadedDate { get; set; } = DateTime.Now;
    }
}
