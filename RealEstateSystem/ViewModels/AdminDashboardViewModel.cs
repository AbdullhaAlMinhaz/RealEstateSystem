using System.Collections.Generic;
using RealEstateSystem.Models;

namespace RealEstateSystem.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalBuyers { get; set; }
        public int TotalSellers { get; set; }
        public int ActiveListings { get; set; }
    }
}
