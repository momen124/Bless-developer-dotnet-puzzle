using System;
using System.Collections.Generic;
using System.Globalization;

namespace SalesTax
{
    public static class InputParser
    {
        public static SaleLine ProcessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 4)
                return null;

            if (!int.TryParse(words[0], out var quantity) || quantity <= 0)
                return null;

            if (!string.Equals(words[^2], "at", StringComparison.OrdinalIgnoreCase))
                return null;

            if (!decimal.TryParse(words[^1], NumberStyles.Number, CultureInfo.InvariantCulture, out var price) || price <= 0)
                return null;

            var descriptionParts = new List<string>();
            var isImported = false;
            for (var i = 1; i < words.Length - 2; i++)
            {
                if (string.Equals(words[i], "imported", StringComparison.OrdinalIgnoreCase))
                {
                    isImported = true;
                    continue;
                }
                descriptionParts.Add(words[i]);
            }
            var productName = string.Join(" ", descriptionParts);
            if (string.IsNullOrEmpty(productName))
                return null;

            if (isImported)
                productName = "imported " + productName;

            return new SaleLine(quantity, productName, price, isImported);
        }
    }
}
