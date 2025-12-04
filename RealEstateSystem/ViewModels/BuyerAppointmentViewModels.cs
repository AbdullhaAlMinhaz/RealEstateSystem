using System;
using System.Collections.Generic;
using RealEstateSystem.Models;

namespace RealEstateSystem.ViewModels
{
    public class BuyerAppointmentItemViewModel
    {
        public int AppointmentId { get; set; }

        public int PropertyId { get; set; }
        public string PropertyTitle { get; set; }
        public string PrimaryImageUrl { get; set; }

        public string City { get; set; }
        public string AreaOrLocation { get; set; }

        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public AppointmentStatus Status { get; set; }

        public string SellerName { get; set; }
    }

    public class BuyerAppointmentListViewModel
    {
        public List<BuyerAppointmentItemViewModel> Upcoming { get; set; } =
            new List<BuyerAppointmentItemViewModel>();

        public List<BuyerAppointmentItemViewModel> Past { get; set; } =
            new List<BuyerAppointmentItemViewModel>();
    }
}
