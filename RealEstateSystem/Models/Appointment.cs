using System;
using System.ComponentModel.DataAnnotations;

namespace RealEstateSystem.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public int BuyerId { get; set; }
        public Buyer Buyer { get; set; }

        public int SellerId { get; set; }
        public Seller Seller { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        public TimeSpan AppointmentTime { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Requested;

        public string Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }
    }
}
