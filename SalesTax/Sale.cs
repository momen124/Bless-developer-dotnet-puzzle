using System.Collections.Generic;
using System.Linq;

namespace SalesTax
{
    public class Sale
    {
        private readonly List<SaleLine> saleLines = new List<SaleLine>();
        private decimal totalTax;
        private decimal totalValue;

        public bool Add(string inputLine)
        {
            var saleLine = InputParser.ProcessInput(inputLine);
            if (saleLine == null)
                return false;
            saleLines.Add(saleLine);
            totalTax += saleLine.Tax;
            totalValue += saleLine.LineValue;
            return true;
        }

        public decimal Tax => totalTax;

        public decimal TotalValue => totalValue;

        public override string ToString()
        {
            var receiptLines = saleLines.Select(line => line.ToString()).ToList();
            receiptLines.Add($"Sales Taxes: {Tax:#,##0.00}");
            receiptLines.Add($"Total: {TotalValue:#,##0.00}");

            return string.Join("\n", receiptLines);
        }
    }
}
