using System;
using System.Collections.Generic;
using System.Text;

namespace SalesTax
{
    public class Sale
    {
        private List<SaleLine> saleLines = new List<SaleLine>();
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
            var output = new StringBuilder();
            foreach (var line in saleLines)
            {
                if (output.Length > 0)
                    output.Append("\n");
                output.Append(line.ToString());
            }
            output.Append("\n");
            output.AppendFormat("Sales Taxes: {0:#,##0.00}", Tax);
            output.Append("\n");
            output.AppendFormat("Total: {0:#,##0.00}", TotalValue);
            return output.ToString();
        }
    }
}
