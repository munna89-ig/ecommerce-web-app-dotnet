namespace Pearl_Poetry.ViewModels.Cart
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();

        public bool IsEmpty => Items.Count == 0;

        public int ItemCount => Items.Sum(i => i.Quantity);

        public bool HasProblems => Items.Any(i => !i.IsAvailable);

        // only lines that can really be bought are counted
        public decimal Subtotal => Items.Where(i => i.IsAvailable).Sum(i => i.LineTotal);
    }

    // ---------- JSON bodies sent by cart.js / site.js ----------
    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int? VariantId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartRequest
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class RemoveCartRequest
    {
        public int ItemId { get; set; }
    }

    // ---------- JSON answer ----------
    public class CartActionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        // total pieces in the cart (header badge)
        public int Count { get; set; }

        // after "update": the quantity that was really applied
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
        public decimal Subtotal { get; set; }

        public static CartActionResult Fail(string message) => new() { Success = false, Message = message };
    }
}