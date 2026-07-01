namespace SalesTax.Tests
{
    public class SalesTaxTests
    {
        private const string Test1Receipt = "1 book: 12.49\n1 music CD: 16.49\n1 packet of chips: 0.85\nSales Taxes: 1.50\nTotal: 29.83";
        private const string Test2Receipt = "1 imported box of chips: 10.50\n1 imported transformer: 54.65\nSales Taxes: 7.65\nTotal: 65.15";
        private const string Test3Receipt = "1 imported barrell of oil: 32.19\n1 bottle of perfume: 20.89\n1 packet of headache tablets: 9.75\n1 imported box of chocolates: 11.85\nSales Taxes: 6.70\nTotal: 74.68";
        private const string Test4Receipt = "10 imported bottles of whiskey: 321.90\n10 bottles of local whiskey: 208.90\n10 packets of iodine tablets: 97.50\n10 imported boxes of potato chips: 118.15\nSales Taxes: 66.65\nTotal: 746.45";
        private const string BlankReceipt = "Sales Taxes: 0.00\nTotal: 0.00";

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
            Assert.Equal(Test1Receipt, Clean(sale.ToString()));
        }

        [Fact]
        public void Test2_ImportedProducts()
        {
            var sale = RunSale(
                "1 imported box of chips at 10.00",
                "1 imported transformer at 47.50"
            );
            Assert.Equal(Test2Receipt, Clean(sale.ToString()));
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
            Assert.Equal(Test3Receipt, Clean(sale.ToString()));
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
            Assert.Equal(Test4Receipt, Clean(sale.ToString()));
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
            Assert.Equal(BlankReceipt, Clean(sale.ToString()));
        }

        [Theory]
        [InlineData("")]
        [InlineData("js s jss s")]
        [InlineData("1 book 12.49")]
        [InlineData("0 book at 12.49")]
        [InlineData("-1 book at 12.49")]
        [InlineData("1 book at 0.00")]
        [InlineData("1 book at -12.49")]
        public void Parser_RejectsInvalidInput(string input)
        {
            Assert.Null(InputParser.ProcessInput(input));
        }

        [Fact]
        public void Parser_AllowsRepeatedSpacesAndCaseInsensitiveImported()
        {
            var line = InputParser.ProcessInput("1   IMPORTED   box   of   chips   AT   10.00");

            Assert.NotNull(line);
            Assert.Equal(1, line.Quantity);
            Assert.Equal("imported box of chips", line.ProductName);
            Assert.True(line.IsImported);
            Assert.Equal(10.00m, line.Price);
        }

        [Theory]
        [InlineData(14.99, 10, 1.50)]
        [InlineData(47.50, 15, 7.15)]
        [InlineData(11.25, 5, 0.60)]
        [InlineData(10.00, 5, 0.50)]
        public void CalculateTax_RoundsUpToNearestFiveCents(decimal value, int taxRate, decimal expected)
        {
            Assert.Equal(expected, SaleLine.CalculateTax(value, taxRate));
        }

        [Fact]
        public void SaleLine_RejectsInvalidConstructorArguments()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SaleLine(0, "book", 12.49m, false));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SaleLine(1, "book", 0m, false));
            Assert.Throws<ArgumentException>(() => new SaleLine(1, "", 12.49m, false));
        }
    }
}
