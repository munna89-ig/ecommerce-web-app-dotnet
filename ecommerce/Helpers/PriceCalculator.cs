namespace Pearl_Poetry.Helpers
{
    public static class PriceCalculator
    {
        // Returns the actual selling price.
        // If DiscountPrice exists and is lower than Price,
        // DiscountPrice is used.
        public static decimal GetSellingPrice(
            decimal price,
            decimal? discountPrice)
        {
            if (discountPrice.HasValue &&
                discountPrice.Value > 0 &&
                discountPrice.Value < price)
            {
                return discountPrice.Value;
            }

            return price;
        }


        // Calculate discount amount for one item
        public static decimal GetDiscountAmount(
            decimal price,
            decimal? discountPrice,
            int quantity)
        {
            decimal sellingPrice =
                GetSellingPrice(price, discountPrice);

            decimal discountPerItem =
                price - sellingPrice;

            return discountPerItem * quantity;
        }


        // Calculate item total
        public static decimal GetItemTotal(
            decimal price,
            decimal? discountPrice,
            int quantity)
        {
            decimal sellingPrice =
                GetSellingPrice(price, discountPrice);

            return sellingPrice * quantity;
        }


        // Calculate order subtotal
        public static decimal CalculateSubtotal(
            IEnumerable<(decimal Price, decimal? DiscountPrice, int Quantity)> items)
        {
            decimal subtotal = 0;

            foreach (var item in items)
            {
                subtotal += GetItemTotal(
                    item.Price,
                    item.DiscountPrice,
                    item.Quantity
                );
            }

            return subtotal;
        }


        // Calculate final order total
        public static decimal CalculateTotal(
            decimal subtotal,
            decimal discountAmount,
            decimal deliveryCharge)
        {
            decimal total =
                subtotal
                - discountAmount
                + deliveryCharge;

            // Never allow negative total
            if (total < 0)
            {
                total = 0;
            }

            return total;
        }


        // Calculate percentage discount
        public static decimal CalculateDiscountPercentage(
            decimal originalPrice,
            decimal sellingPrice)
        {
            if (originalPrice <= 0)
            {
                return 0;
            }

            decimal discount =
                ((originalPrice - sellingPrice)
                / originalPrice) * 100;

            return Math.Round(
                discount,
                2
            );
        }
    }
}