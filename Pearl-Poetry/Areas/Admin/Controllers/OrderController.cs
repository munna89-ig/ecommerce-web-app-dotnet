using Microsoft.AspNetCore.Mvc;
using Pearl_Poetry.Enums;
using Pearl_Poetry.Interfaces;
using System.Security.Claims;
using Pearl_Poetry.Services;

namespace Pearl_Poetry.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly InvoicePdfService _invoicePdfService;


        public OrderController(IOrderService orderService, InvoicePdfService invoicePdfService)
        {
            _orderService = orderService;
            _invoicePdfService = invoicePdfService;
        }


        // =========================================================
        // ORDER LIST
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            int page = 1,
            string? search = null,
            string? status = null,
            string? paymentStatus = null)
        {
            int pageSize = 10;

            var result = await _orderService.GetOrdersAsync(
                page,
                pageSize,
                search,
                status,
                paymentStatus);


            ViewBag.TotalItems = result.TotalItems;

            ViewBag.TotalPages =
                (int)Math.Ceiling(
                    result.TotalItems / (double)pageSize);

            ViewBag.CurrentPage = page;

            ViewBag.Search = search;

            ViewBag.Status = status;

            ViewBag.PaymentStatus = paymentStatus;


            ViewBag.OrderStatuses =
                Enum.GetNames<OrderStatus>()
                    .Select(x => x.ToLower())
                    .ToList();


            return View(result.Orders);
        }


        // =========================================================
        // ORDER DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order =
                await _orderService.GetOrderDetailsAsync(id);


            if (order == null)
            {
                TempData["Error"] = "Order not found.";

                return RedirectToAction(nameof(Index));
            }


            return View(order);
        }


        // =========================================================
        // UPDATE ORDER STATUS
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int id,
            string status,
            string? note)
        {
            if (!Enum.TryParse<OrderStatus>(
                    status,
                    true,
                    out var newStatus))
            {
                TempData["Error"] =
                    "Invalid order status.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            int? adminId = GetCurrentUserId();


            bool success =
                await _orderService.UpdateOrderStatusAsync(
                    id,
                    newStatus,
                    adminId,
                    note);


            if (!success)
            {
                TempData["Error"] =
                    "Order not found.";
            }
            else
            {
                TempData["Success"] =
                    "Order status updated successfully.";
            }


            return RedirectToAction(
                nameof(Details),
                new { id });
        }


        // =========================================================
        // UPDATE PAYMENT STATUS
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePaymentStatus(
            int id,
            string paymentStatus)
        {
            if (string.IsNullOrWhiteSpace(paymentStatus))
            {
                TempData["Error"] =
                    "Payment status is required.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }


            bool success =
                await _orderService.UpdatePaymentStatusAsync(
                    id,
                    paymentStatus);


            if (!success)
            {
                TempData["Error"] =
                    "Order not found.";
            }
            else
            {
                TempData["Success"] =
                    "Payment status updated successfully.";
            }


            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        //Invoice 
        [HttpGet]
        public async Task<IActionResult> Invoice(int id)
        {
            var order = await _orderService.GetOrderDetailsAsync(id);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            byte[] pdf = _invoicePdfService.GenerateInvoice(order);

            string fileName = $"Invoice-{order.OrderNumber}.pdf";

            return File(
                pdf,
                "application/pdf",
                fileName);
        }


        // =========================================================
        // CURRENT ADMIN USER ID
        // =========================================================

        private int? GetCurrentUserId()
        {
            string? userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            if (int.TryParse(userId, out int id))
                return id;


            return null;
        }
    }
}