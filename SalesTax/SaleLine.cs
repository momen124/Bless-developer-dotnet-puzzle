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
            if (lineQuantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(lineQuantity), "Quantity must be greater than zero.");
            if (unitPrice <= 0)
                throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price must be greater than zero.");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name is required.", nameof(name));

            Quantity = lineQuantity;
            ProductName = name;
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
            // Kata-scope classification: enough for supplied products, not a production catalog.
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
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Taxable value cannot be negative.");
            if (taxRate < 0)
                throw new ArgumentOutOfRangeException(nameof(taxRate), "Tax rate cannot be negative.");

            var rawTax = value * taxRate / 100m;
            return Math.Ceiling(rawTax / 0.05m) * 0.05m;
        }

        public override string ToString()
        {
            return $"{Quantity} {ProductName}: {LineValue:0.00}";
        }
    }
}
