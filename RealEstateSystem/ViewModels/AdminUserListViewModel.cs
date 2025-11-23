using System.Collections.Generic;
using RealEstateSystem.Models;

namespace RealEstateSystem.ViewModels
{
    public class AdminUserListViewModel
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }

        public IEnumerable<User> Users { get; set; }
    }
}
