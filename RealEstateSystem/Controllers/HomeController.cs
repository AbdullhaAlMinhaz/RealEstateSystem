using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;
using RealEstateSystem.ViewModels;

namespace RealEstateSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Landing page (dynamic properties + search + pagination)
        public IActionResult Index(
            string location,
            PropertyType? propertyType,
            decimal? minPrice,
            decimal? maxPrice,
            int page = 1)
        {
            const int pageSize = 6;

            
            var query = _context.Properties
                .AsNoTracking()
                .Include(p => p.Images)
                .Where(p => p.Status != PropertyStatus.Removed
                         && p.ApprovalStatus != PropertyApprovalStatus.Rejected);

            // Filters
            if (!string.IsNullOrWhiteSpace(location))
            {
                var loc = location.Trim();
                query = query.Where(p =>
                    (p.City != null && p.City.Contains(loc)) ||
                    (p.State != null && p.State.Contains(loc)) ||
                    (p.AreaOrLocation != null && p.AreaOrLocation.Contains(loc)) ||
                    (p.Address != null && p.Address.Contains(loc))
                );
            }

            if (propertyType.HasValue)
                query = query.Where(p => p.PropertyType == propertyType.Value);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            // Sorting: featured first then newest
            query = query
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedDate);

            var totalCount = query.Count();
            var totalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages < 1) totalPages = 1;

            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var properties = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new HomePropertyCardViewModel
                {
                    PropertyId = p.PropertyId,
                    Price = p.Price,
                    Bedrooms = p.Bedrooms,
                    Bathrooms = p.Bathrooms,
                    City = p.City,
                    State = p.State,
                    AreaOrLocation = p.AreaOrLocation,

                    // Try to get primary image first, otherwise first image
                    PrimaryImageUrl = p.Images
                        .OrderByDescending(i => i.IsPrimary)
                        .ThenBy(i => i.DisplayOrder)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault()
                })
                .ToList();

            var vm = new HomeIndexViewModel
            {
                Location = location,
                PropertyType = propertyType,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                Properties = properties
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
