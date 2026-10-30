using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Pearl_Poetry.Data;
using Pearl_Poetry.Data.Entities;
using Pearl_Poetry.Services.Interfaces;
using Pearl_Poetry.ViewModels.Admin.Product;
using System;

namespace Pearl_Poetry.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly EcommerceSystemContext _context;   
        private readonly IWebHostEnvironment _env;

        public CategoryController(EcommerceSystemContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }


        public IActionResult Index()
        {
           
            ViewBag.Categories = _context.Categories
                .Where(c => c.IsActive == true && c.ParentId == null)
                .OrderBy(c => c.Id)
                .ToList();

            var list = _context.Categories
                .Where(c => c.ParentId == null)
                .OrderByDescending(c => c.Id)
                .ToList();

            return View(list);
        }

       
        [HttpPost]
        public async Task<IActionResult> SaveCategory(int id, string name, string? description, bool isActive, IFormFile? image)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "Name is required." });

           
            string slug = System.Text.RegularExpressions.Regex
                .Replace(name.Trim().ToLower(), @"[^a-z0-9]+", "-").Trim('-');

           
            bool slugExists = _context.Categories.Any(c => c.Slug == slug && c.Id != id);
            if (slugExists)
                slug = slug + "-" + DateTime.Now.Ticks.ToString().Substring(10);

            
            string? imagePath = null;
            if (image != null && image.Length > 0)
            {
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var ext = Path.GetExtension(image.FileName).ToLower();
                if (!allowed.Contains(ext))
                    return Json(new { success = false, message = "Only jpg, jpeg, png, webp allowed." });

                var folder = Path.Combine(_env.WebRootPath, "images", "categories");
                Directory.CreateDirectory(folder);

                var fileName = slug + "-" + Guid.NewGuid().ToString("N").Substring(0, 6) + ext;
                using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                imagePath = "/images/categories/" + fileName;
            }

            if (id == 0)
            {
                var cat = new Category
                {
                    ParentId = null,
                    Name = name.Trim(),
                    Slug = slug,
                    Description = description,
                    ImagePath = imagePath,
                    IsActive = isActive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.Categories.Add(cat);
            }
            else
            {
                var cat = _context.Categories.FirstOrDefault(c => c.Id == id);
                if (cat == null)
                    return Json(new { success = false, message = "Category not found." });

                cat.Name = name.Trim();
                cat.Slug = slug;
                cat.Description = description;
                cat.IsActive = isActive;
                cat.UpdatedAt = DateTime.Now;
                if (imagePath != null) cat.ImagePath = imagePath;   
            }

            _context.SaveChanges();
            return Json(new { success = true });
        }

        
        [HttpPost]
        public IActionResult DeleteCategory(int id)
        {
            var cat = _context.Categories.FirstOrDefault(c => c.Id == id);
            if (cat == null)
                return Json(new { success = false, message = "Category not found." });

            bool hasProducts = _context.Products.Any(p => p.CategoryId == id && p.DeletedAt == null);
            if (hasProducts)
                return Json(new { success = false, message = "Category name already exist!" });

            _context.Categories.Remove(cat);
            _context.SaveChanges();
            return Json(new { success = true });
        }

    }
}