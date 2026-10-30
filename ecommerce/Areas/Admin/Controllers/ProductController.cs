using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pearl_Poetry.Data;
using Pearl_Poetry.Services.Interfaces;
using Pearl_Poetry.ViewModels.Admin.Product;

namespace Pearl_Poetry.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly EcommerceSystemContext _context;

        public ProductController(
            IProductService productService,
            EcommerceSystemContext context)
        {
            _productService = productService;
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> Index(
            int page = 1)
        {
            var model =
                await _productService
                    .GetProductsAsync(page, 10);

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var product =
                await _productService
                    .GetDetailsAsync(id);

            if (product == null)
                return NotFound();

            return View(product);
        }


        [HttpGet]
        public IActionResult AddProduct()
        {
            LoadCategories();

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(
            ProductCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                LoadCategories();

                return View(vm);
            }

            try
            {
                await _productService
                    .CreateAsync(vm);

                TempData["Success"] =
                    "Product added successfully!";

                return RedirectToAction(
                    nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    "",
                    ex.Message);

                LoadCategories();

                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(
            int id)
        {
            var model =
                await _productService
                    .GetEditAsync(id);

            if (model == null)
                return NotFound();

            LoadCategories();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(
            ProductEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                LoadCategories();

                return View(vm);
            }

            try
            {
                bool updated =
                    await _productService
                        .UpdateAsync(vm);

                if (!updated)
                    return NotFound();

                TempData["Success"] =
                    "Product updated successfully!";

                return RedirectToAction(
                    nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    "",
                    ex.Message);

                LoadCategories();

                return View(vm);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            try
            {
                bool deleted = await _productService.DeleteAsync(id);

                if (isAjax)
                {
                    return Json(deleted
                        ? new { success = true, message = (string?)null }
                        : new { success = false, message = (string?)"Product not found." });
                }

                TempData[deleted ? "Success" : "Error"] = deleted
                    ? "Product deleted successfully."
                    : $"Product #{id} not found.";
            }
            catch (Exception ex)
            {
                // show the real reason instead of failing silently
                string reason = ex.GetBaseException().Message;

                if (isAjax)
                    return Json(new { success = false, message = "Delete failed: " + reason });

                TempData["Error"] = "Delete failed: " + reason;
            }

            return RedirectToAction(nameof(Index));
        }

        private void LoadCategories()
        {
            ViewBag.Categories =
                _context.Categories
                    .Where(c => c.IsActive == true)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    })
                    .ToList();
        }
    }
}