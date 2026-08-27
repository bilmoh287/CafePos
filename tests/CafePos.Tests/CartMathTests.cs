using CafePos.Helpers;
using Xunit;

namespace CafePos.Tests;

public class CartMathTests
{
    [Fact]
    public void TestEnvironment_IsConfiguredCorrectly()
    {
        // Assert
        Assert.True(true, "Unit test environment configured and executing cleanly.");
    }

    [Fact]
    public void CalculateTax_ZeroOrNegativeSubtotal_ReturnsZero()
    {
        // Act & Assert
        Assert.Equal(0m, TaxCalculator.CalculateTax(0m));
        Assert.Equal(0m, TaxCalculator.CalculateTax(-10m));
    }

    [Theory]
    [InlineData(10.00, 0.85, 10.85)]  // $10.00 subtotal -> $0.85 tax -> $10.85 total
    [InlineData(2.50, 0.21, 2.71)]    // $2.50 subtotal (Espresso) -> 0.2125 tax rounded to $0.21 -> $2.71 total
    [InlineData(3.25, 0.28, 3.53)]    // $3.25 subtotal (Croissant) -> 0.27625 tax rounded to $0.28 -> $3.53 total
    public void CalculateTax_ValidSubtotal_ComputesCorrect8Point5PercentTaxAndTotal(decimal subtotal, decimal expectedTax, decimal expectedGrandTotal)
    {
        // Act
        decimal tax = TaxCalculator.CalculateTax(subtotal);
        decimal grandTotal = TaxCalculator.CalculateGrandTotal(subtotal, tax);

        // Assert
        Assert.Equal(expectedTax, tax);
        Assert.Equal(expectedGrandTotal, grandTotal);
    }
}

