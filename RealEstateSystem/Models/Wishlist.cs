using System;
using System.ComponentModel.DataAnnotations;

namespace RealEstateSystem.Models
{
    public class Wishlist
    {
        [Key]
        public int WishlistId { get; set; }

        public int BuyerId { get; set; }
        public Buyer Buyer { get; set; }

        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.Now;

        public string Notes { get; set; } = string.Empty;
    }
}
