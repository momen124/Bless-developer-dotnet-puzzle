namespace SalesTax.Tests
{
    public class SalesTaxTests
    {
        private static Sale RunSale(params string[] lines)
        {
            var sale = new Sale();
            foreach (var line in lines)
                sale.Add(line);
            return sale;
        }

        private static string Clean(string s) =>
            s.Replace("\r\n", "\n").Replace("\r", "\n");

        [Fact]
        public void Test1_BasicExemptAndTaxable()
        {
            var sale = RunSale(
                "1 book at 12.49",
                "1 music CD at 14.99",
                "1 packet of chips at 0.85"
            );
            var output = Clean(sale.ToString());
            Assert.Contains("1 book: 12.49", output);
            Assert.Contains("1 music CD: 16.49", output);
            Assert.Contains("1 packet of chips: 0.85", output);
            Assert.Contains("Sales Taxes: 1.50", output);
            Assert.Contains("Total: 29.83", output);
        }

        [Fact]
        public void Test2_ImportedProducts()
        {
            var sale = RunSale(
                "1 imported box of chips at 10.00",
                "1 imported transformer at 47.50"
            );
            var output = Clean(sale.ToString());
            Assert.Contains("1 imported box of chips: 10.50", output);
            Assert.Contains("1 imported transformer: 54.65", output);
            Assert.Contains("Sales Taxes: 7.65", output);
            Assert.Contains("Total: 65.15", output);
        }

        [Fact]
        public void Test3_MixedBasket()
        {
            var sale = RunSale(
                "1 barrell of imported oil at 27.99",
                "1 bottle of perfume at 18.99",
                "1 packet of headache tablets at 9.75",
                "1 box of imported chocolates at 11.25"
            );
            var output = Clean(sale.ToString());
            Assert.Contains("1 imported barrell of oil: 32.19", output);
            Assert.Contains("1 bottle of perfume: 20.89", output);
            Assert.Contains("1 packet of headache tablets: 9.75", output);
            Assert.Contains("1 imported box of chocolates: 11.85", output);
            Assert.Contains("Sales Taxes: 6.70", output);
            Assert.Contains("Total: 74.68", output);
        }

        [Fact]
        public void Test4_MultipleQuantities()
        {
            var sale = RunSale(
                "10 imported bottles of whiskey at 27.99",
                "10 bottles of local whiskey at 18.99",
                "10 packets of iodine tablets at 9.75",
                "10 boxes of imported potato chips at 11.25"
            );
            var output = Clean(sale.ToString());
            Assert.Contains("10 imported bottles of whiskey: 321.90", output);
            Assert.Contains("10 bottles of local whiskey: 208.90", output);
            Assert.Contains("10 packets of iodine tablets: 97.50", output);
            Assert.Contains("10 imported boxes of potato chips: 118.15", output);
            Assert.Contains("Sales Taxes: 66.65", output);
            Assert.Contains("Total: 746.45", output);
        }

        [Fact]
        public void Test5_InvalidInputReturnsFalse()
        {
            var sale = new Sale();
            Assert.False(sale.Add("js s jss s"));
        }

        [Fact]
        public void Test6_BlankSaleShowsZeroTotals()
        {
            var sale = new Sale();
            var output = Clean(sale.ToString());
            Assert.Contains("Sales Taxes: 0.00", output);
            Assert.Contains("Total: 0.00", output);
        }
    }
}
