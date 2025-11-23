using System.Collections.Generic;
using RealEstateSystem.Models;

namespace RealEstateSystem.ViewModels
{
    public class AdminPropertyListViewModel
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }

        public IEnumerable<Property> Properties { get; set; }
    }
}
