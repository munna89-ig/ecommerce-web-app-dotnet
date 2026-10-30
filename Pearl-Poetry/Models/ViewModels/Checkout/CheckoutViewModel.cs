using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Pearl_Poetry.ViewModels.Cart;

namespace Pearl_Poetry.ViewModels.Checkout
{
    public class CheckoutViewModel
    {
        // ---------- "Buy now" mode (empty => checkout the whole cart) ----------
        public int? BuyNowProductId { get; set; }
        public int? BuyNowVariantId { get; set; }
        public int BuyNowQuantity { get; set; } = 1;

        public bool IsBuyNow => BuyNowProductId.HasValue;

        // ---------- form fields ----------
        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(150)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Please enter your phone number.")]
        [RegularExpression(@"^(\+?88)?01[3-9]\d{8}$",
            ErrorMessage = "Enter a valid mobile number, e.g. 01712345678.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select your district.")]
        public string District { get; set; } = "Dhaka";

        [Required(ErrorMessage = "Please enter your full address.")]
        [StringLength(300)]
        public string Address { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(500)]
        public string? DeliveryNote { get; set; }

        // "cod" | "bkash"
        [Required]
        public string PaymentMethod { get; set; } = "cod";

        // bKash only
        [StringLength(20)]
        public string? SenderPhone { get; set; }

        [StringLength(100)]
        public string? SenderName { get; set; }

        [StringLength(150)]
        public string? TransactionId { get; set; }

        // ---------- display only (filled by the server, never validated) ----------
        [ValidateNever]
        public List<CartItemViewModel> Items { get; set; } = new();

        [ValidateNever] public decimal Subtotal { get; set; }
        [ValidateNever] public decimal DeliveryCharge { get; set; }
        [ValidateNever] public decimal Total { get; set; }

        [ValidateNever] public decimal InsideDhakaCharge { get; set; }
        [ValidateNever] public decimal OutsideDhakaCharge { get; set; }

        [ValidateNever] public string? BkashNumber { get; set; }
        [ValidateNever] public string? BkashQrImage { get; set; }

        public bool HasProblems => Items.Any(i => !i.IsAvailable);

        public bool CanPlaceOrder => Items.Count > 0 && !HasProblems;

        public static readonly string[] Districts =
        {
            "Dhaka", "Gazipur", "Narayanganj", "Narsingdi", "Manikganj", "Munshiganj", "Tangail", "Kishoreganj",
            "Faridpur", "Gopalganj", "Madaripur", "Rajbari", "Shariatpur",
            "Chattogram", "Cox's Bazar", "Cumilla", "Brahmanbaria", "Chandpur", "Feni", "Lakshmipur", "Noakhali",
            "Khagrachhari", "Rangamati", "Bandarban",
            "Rajshahi", "Bogura", "Joypurhat", "Naogaon", "Natore", "Chapainawabganj", "Pabna", "Sirajganj",
            "Khulna", "Bagerhat", "Chuadanga", "Jashore", "Jhenaidah", "Kushtia", "Magura", "Meherpur", "Narail", "Satkhira",
            "Barishal", "Barguna", "Bhola", "Jhalokati", "Patuakhali", "Pirojpur",
            "Sylhet", "Habiganj", "Moulvibazar", "Sunamganj",
            "Rangpur", "Dinajpur", "Gaibandha", "Kurigram", "Lalmonirhat", "Nilphamari", "Panchagarh", "Thakurgaon",
            "Mymensingh", "Jamalpur", "Netrokona", "Sherpur"
        };
    }

    public class PlaceOrderResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? OrderNumber { get; set; }

        public static PlaceOrderResult Ok(string orderNumber) => new() { Success = true, OrderNumber = orderNumber };
        public static PlaceOrderResult Fail(string error) => new() { Success = false, Error = error };
    }

    public class OrderSuccessViewModel
    {
        public string OrderNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public string PaymentMethodLabel { get; set; } = string.Empty;
        public bool PaymentNeedsVerification { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DeliveryCharge { get; set; }
        public decimal Total { get; set; }
        public List<OrderSuccessItem> Items { get; set; } = new();
    }

    public class OrderSuccessItem
    {
        public string Title { get; set; } = string.Empty;
        public string? VariantLabel { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}