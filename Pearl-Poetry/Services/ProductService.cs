using Microsoft.EntityFrameworkCore;
using Pearl_Poetry.Data;
using Pearl_Poetry.Data.Entities;
using Pearl_Poetry.Services.Interfaces;
using Pearl_Poetry.ViewModels.Admin.Product;
using Pearl_Poetry.Enums;
using Pearl_Poetry.Models.Enums;

namespace Pearl_Poetry.Services
{
    public class ProductService : IProductService
    {
        private readonly EcommerceSystemContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductService(
            EcommerceSystemContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // =========================================================
        // PRODUCT LIST
        // =========================================================

        public async Task<ProductListViewModel> GetProductsAsync(
            int page = 1,
            int pageSize = 10)
        {
            if (page < 1)
                page = 1;

            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.DeletedAt == null)
                .OrderByDescending(p => p.Id);

            int totalItems = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductListItemViewModel
                {
                    Id = p.Id,
                    Title = p.Title ?? string.Empty,
                    Sku = p.Sku,
                    CategoryName = p.Category != null
                        ? p.Category.Name
                        : null,
                    Price = p.Price,
                    DiscountPrice = p.DiscountPrice,
                    Quantity = (int)p.Quantity,
                    CoverImagePath = p.CoverImagePath,
                    IsFeatured = (bool)p.IsFeatured,
                    IsActive = (bool)p.IsActive
                })
                .ToListAsync();

            int totalPages =
                (int)Math.Ceiling(
                    totalItems / (double)pageSize);

            return new ProductListViewModel
            {
                Products = products,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }


        // =========================================================
        // PRODUCT DETAILS
        // =========================================================

        public async Task<ProductDetailsViewModel?> GetDetailsAsync(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.DeletedAt == null);

            if (product == null)
                return null;

            return new ProductDetailsViewModel
            {
                Id = product.Id,

                Title = product.Title ?? string.Empty,

                Slug = product.Slug ?? string.Empty,

                Sku = product.Sku,

                ProductType = product.ProductType,

                CategoryName = product.Category?.Name,

                Description = product.Description,

                ShortDescription = product.ShortDescription,

                Price = product.Price,

                DiscountPrice = product.DiscountPrice,

                Quantity = (int)product.Quantity,

                CoverImagePath = product.CoverImagePath,

                IsFeatured = (bool)product.IsFeatured,

                IsActive = (bool)product.IsActive,

                CreatedAt = product.CreatedAt,

                UpdatedAt = product.UpdatedAt,

                Images = product.ProductImages
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new ProductImageViewModel
                    {
                        Id = i.Id,
                        ImagePath = i.ImagePath,
                        SortOrder = (int)i.SortOrder
                    })
                    .ToList(),

                Variants = product.ProductVariants
                    .OrderBy(v => v.Id)
                    .Select(v => new ProductVariantViewModel
                    {
                        Id = v.Id,
                        Sku = v.Sku,
                        Color = v.Color,
                        Size = v.Size,
                        Price = v.Price,
                        DiscountPrice = v.DiscountPrice,
                        Quantity = (int)v.Quantity,
                        IsActive = (bool)v.IsActive
                    })
                    .ToList()
            };
        }


        // =========================================================
        // GET EDIT FORM
        // =========================================================

        public async Task<ProductEditViewModel?> GetEditAsync(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.DeletedAt == null);

            if (product == null)
                return null;

            return new ProductEditViewModel
            {
                Id = product.Id,

                CategoryId = (int)product.CategoryId,

                Title = product.Title ?? string.Empty,

                Sku = product.Sku ?? string.Empty,

                ProductType = Enum.TryParse<
                    ProductType>(
                        product.ProductType,
                        true,
                        out var productType)
                    ? productType
                    : default,

                Description = product.Description,

                ShortDescription = product.ShortDescription,

                Price = product.Price,

                DiscountPrice = product.DiscountPrice,

                Quantity = (int)product.Quantity,

                IsFeatured = (bool)product.IsFeatured,

                IsActive = (bool)product.IsActive,

                ExistingCoverImagePath =
                    product.CoverImagePath
            };
        }


        // =========================================================
        // UPDATE PRODUCT
        // =========================================================

        public async Task<bool> UpdateAsync(
            ProductEditViewModel model)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == model.Id &&
                    p.DeletedAt == null);

            if (product == null)
                return false;


            // Check duplicate SKU
            bool duplicateSku = await _context.Products
                .AnyAsync(p =>
                    p.Id != model.Id &&
                    p.Sku == model.Sku &&
                    p.DeletedAt == null);

            if (duplicateSku)
                throw new InvalidOperationException(
                    "Another product already uses this SKU.");


            // Update basic information
            product.CategoryId = model.CategoryId;

            product.Title = model.Title;

            product.Sku = model.Sku;

            product.ProductType =
                model.ProductType
                    .ToString()
                    .ToLowerInvariant();

            product.Description = model.Description;

            product.ShortDescription =
                model.ShortDescription;

            product.Price = model.Price;

            product.DiscountPrice =
                model.DiscountPrice;

            product.Quantity = model.Quantity;

            product.IsFeatured =
                model.IsFeatured;

            product.IsActive =
                model.IsActive;


            // Generate new slug if title changed
            product.Slug = await GenerateUniqueSlugAsync(
                model.Title,
                model.Id);


            // Upload new image
            if (model.CoverImage != null)
            {
                string? oldImage =
                    product.CoverImagePath;

                string newImagePath =
                    await SaveImageAsync(
                        model.CoverImage);

                product.CoverImagePath =
                    newImagePath;

                // Delete old image
                DeletePhysicalImage(oldImage);
            }


            product.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;
        }


        // =========================================================
        // DELETE PRODUCT - SOFT DELETE
        // =========================================================

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return false;

            product.DeletedAt = DateTime.Now;

            product.IsActive = false;

            await _context.SaveChangesAsync();

            return true;
        }


        // =========================================================
        // CREATE PRODUCT
        // =========================================================

        public async Task<bool> CreateAsync(
            ProductCreateViewModel model)
        {
            string? imagePath = null;

            if (model.CoverImage != null)
            {
                imagePath =
                    await SaveImageAsync(
                        model.CoverImage);
            }

            string slug =
                await GenerateUniqueSlugAsync(
                    model.Title,
                    null);

            var product = new Product
            {
                CategoryId = model.CategoryId,

                Title = model.Title,

                Slug = slug,

                ProductType =
                    model.ProductType
                        .ToString()
                        .ToLowerInvariant(),

                Description =
                    model.Description,

                ShortDescription =
                    model.ShortDescription,

                Price = model.Price,

                DiscountPrice =
                    model.DiscountPrice,

                Quantity =
                    model.Quantity,

                CoverImagePath =
                    imagePath,

                Sku = model.Sku,

                IsFeatured =
                    model.IsFeatured,

                IsActive =
                    model.IsActive,

                CreatedAt = DateTime.Now
            };

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            return true;
        }


        // =========================================================
        // IMAGE UPLOAD
        // =========================================================

        private async Task<string> SaveImageAsync(
            IFormFile image)
        {
            string uploadsFolder =
                Path.Combine(
                    _environment.WebRootPath,
                    "images",
                    "products");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(
                    uploadsFolder);
            }

            string extension =
                Path.GetExtension(
                    image.FileName);

            string fileName =
                Guid.NewGuid().ToString()
                + extension;

            string fullPath =
                Path.Combine(
                    uploadsFolder,
                    fileName);

            using var stream =
                new FileStream(
                    fullPath,
                    FileMode.Create);

            await image.CopyToAsync(stream);

            return "/images/products/" + fileName;
        }


        // =========================================================
        // DELETE PHYSICAL IMAGE
        // =========================================================

        private void DeletePhysicalImage(
            string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            string relativePath =
                imagePath.TrimStart('/')
                    .Replace(
                        '/',
                        Path.DirectorySeparatorChar);

            string fullPath =
                Path.Combine(
                    _environment.WebRootPath,
                    relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }


        // =========================================================
        // UNIQUE SLUG
        // =========================================================

        private async Task<string> GenerateUniqueSlugAsync(
            string title,
            int? currentProductId)
        {
            string baseSlug =
                title.Trim()
                    .ToLowerInvariant()
                    .Replace(" ", "-");

            // Remove basic unwanted characters
            baseSlug = new string(
                baseSlug
                    .Where(c =>
                        char.IsLetterOrDigit(c)
                        || c == '-')
                    .ToArray());

            string slug = baseSlug;

            int counter = 1;

            while (await _context.Products.AnyAsync(p =>
                p.Slug == slug &&
                p.Id != currentProductId))
            {
                slug =
                    $"{baseSlug}-{counter}";

                counter++;
            }

            return slug;
        }
    }
}