using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Pearl_Poetry.Data;
using Pearl_Poetry.Models;

namespace Pearl_Poetry.Controllers
{
    public class CategoryController (EcommerceSystemContext _context): Controller
    {
        public IActionResult Index()
        { 
            var categories = _context.Categories
                    .Where(c => c.IsActive == true)
                    .ToList();

                return View(categories);
            
        }
    }
}