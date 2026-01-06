//using System;
//using RealEstateSystem.Models;

//namespace RealEstateSystem.ViewModels
//{
//    public class PendingPropertyApprovalViewModel
//    {
//        public int PropertyId { get; set; }
//        public string Title { get; set; }
//        public string SellerName { get; set; }
//        public DateTime SubmittedAt { get; set; }
//        public PropertyApprovalStatus ApprovalStatus { get; set; }
//    }
//}


using System;
using RealEstateSystem.Models;

namespace RealEstateSystem.ViewModels
{
    public class PendingPropertyApprovalViewModel
    {
        public int PropertyId { get; set; }
        public string Title { get; set; }
        public string SellerName { get; set; }
        public DateTime SubmittedAt { get; set; }
        public PropertyApprovalStatus ApprovalStatus { get; set; }

        // ✅ For Commission Modal (New)
        public string Address { get; set; }
        public string City { get; set; }
        public string PropertyType { get; set; }
        public int CommissionRatePercent { get; set; } = 2;
    }
}
