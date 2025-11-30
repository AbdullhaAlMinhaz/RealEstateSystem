using System.Collections.Generic;
using RealEstateSystem.Models;

namespace RealEstateSystem.ViewModels
{
    public class PropertyDetailsViewModel
    {
        public Property Property { get; set; }

        public IEnumerable<PropertyImage> Images { get; set; }

        public string SellerName { get; set; }
        public string SellerEmail { get; set; }
        public string SellerPhone { get; set; }
    }
}
