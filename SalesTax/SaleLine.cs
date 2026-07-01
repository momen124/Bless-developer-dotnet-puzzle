using System;

namespace SalesTax
{
    public class SaleLine
    {
        public string ProductName { get; }
        public decimal Price { get; }
        public bool IsImported { get; }
        public int Quantity { get; }
        public decimal LineValue { get; private set; }
        public decimal Tax { get; private set; }

        public SaleLine(int lineQuantity, string name, decimal unitPrice, bool itemIsImported)
        {
            Quantity = lineQuantity;
            ProductName = name ?? string.Empty;
            Price = unitPrice;
            IsImported = itemIsImported;

            var rawValue = Price * Quantity;

            var taxRate = 0;
            if (!IsExempt(ProductName))
                taxRate += 10;
            if (IsImported)
                taxRate += 5;

            Tax = CalculateTax(rawValue, taxRate);
            LineValue = rawValue + Tax;
        }

        private static bool IsExempt(string name)
        {
            var keywords = new[] { "book", "chocolate", "chip", "tablet" };
            foreach (var kw in keywords)
            {
                if (name.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        public static decimal CalculateTax(decimal value, int taxRate)
        {
            var rawTax = value * taxRate / 100m;
            return Math.Ceiling(rawTax / 0.05m) * 0.05m;
        }

        public override string ToString()
        {
            return $"{Quantity} {ProductName}: {LineValue:0.00}";
        }
    }
}
