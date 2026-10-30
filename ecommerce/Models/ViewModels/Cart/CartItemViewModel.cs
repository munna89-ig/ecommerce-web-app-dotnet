namespace Pearl_Poetry.ViewModels.Cart
{
    // One line in the cart / checkout summary.
    public class CartItemViewModel
    {
        // cart_items.id  (0 when the line comes from "Buy now")
        public int ItemId { get; set; }

        public int ProductId { get; set; }

        public int? VariantId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string ImagePath { get; set; } = "/images/categories/fashion.jpg";

        // "Color: Red, Size: M" - null when the variant has no color AND no size,
        // so the views simply render nothing.
        public string? VariantLabel { get; set; }

        // price the customer really pays for ONE piece
        public decimal UnitPrice { get; set; }

        // struck-through price, only set when there is a discount
        public decimal? OriginalPrice { get; set; }

        public int Quantity { get; set; }

        public int Stock { get; set; }

        // null => everything is fine
        public string? Problem { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;

        public bool IsAvailable => Problem == null;
    }
}