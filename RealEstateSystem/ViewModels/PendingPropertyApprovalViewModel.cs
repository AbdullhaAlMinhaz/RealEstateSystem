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
    }
}
