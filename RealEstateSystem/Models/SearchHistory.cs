using System;
using System.ComponentModel.DataAnnotations;

namespace RealEstateSystem.Models
{
    public class SearchHistory
    {
        [Key]
        public int SearchId { get; set; }

        public int BuyerId { get; set; }
        public Buyer Buyer { get; set; }

        public string SearchQuery { get; set; }

        public string Filters { get; set; }

        public int ResultCount { get; set; }

        public DateTime SearchDate { get; set; } = DateTime.Now;
    }
}
