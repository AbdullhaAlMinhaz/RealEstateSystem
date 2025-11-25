//using System.Collections.Generic;
//using RealEstateSystem.Models;

//namespace RealEstateSystem.ViewModels
//{
//    public class AdminDashboardViewModel
//    {
//        public int TotalUsers { get; set; }
//        public int TotalBuyers { get; set; }
//        public int TotalSellers { get; set; }
//        public int ActiveListings { get; set; }
//    }
//}


using System.Collections.Generic;
namespace RealEstateSystem.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalBuyers { get; set; }
        public int TotalSellers { get; set; }
        public int ActiveListings { get; set; }

        // NEW: list of pending properties that will show in the table
        public List<PendingPropertyApprovalViewModel> PendingApprovals { get; set; } =
            new List<PendingPropertyApprovalViewModel>();
    }
}
