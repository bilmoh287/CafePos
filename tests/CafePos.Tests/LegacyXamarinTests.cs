using CafePos.Models;
using CafePos.Models.Legacy;
using Xunit;

namespace CafePos.Tests;

public class LegacyXamarinTests
{
    private readonly Product _croissant = new Product { Id = 1, SKU = "SKU-001", Name = "Croissant", Price = 5.00m, Category = "Bakery" };

    [Fact]
    public void ApplyDiscount_20PercentOn10DollarTotal_Returns8Dollars()
    {
        // Arrange - $10.00 total cart (2x $5.00 Croissants)
        var viewModel = new ModernizedCartViewModel();
        viewModel.Items.Add(new CartItem(_croissant, 2));

        Assert.Equal(10.00m, viewModel.Subtotal);

        // Act - Apply 20% discount
        viewModel.ApplyDiscount(20m);

        // Assert
        Assert.Equal(20m, viewModel.DiscountPercent);
        Assert.Equal(2.00m, viewModel.DiscountAmount);
        Assert.Equal(8.00m, viewModel.Total); // Correct $8.00 (NOT negative -$190.00!)
    }

    [Fact]
    public void ApplyDiscount_BoundaryValidation_ClampsNegativeAndExceedingValues()
    {
        // Arrange
        var viewModel = new ModernizedCartViewModel();
        viewModel.Items.Add(new CartItem(_croissant, 2)); // $10.00 subtotal

        // Act 1 - Negative percentage (-15%)
        viewModel.ApplyDiscount(-15m);
        Assert.Equal(0m, viewModel.DiscountPercent);
        Assert.Equal(10.00m, viewModel.Total);

        // Act 2 - Exceeding percentage (150%)
        viewModel.ApplyDiscount(150m);
        Assert.Equal(100m, viewModel.DiscountPercent);
        Assert.Equal(10.00m, viewModel.DiscountAmount);
        Assert.Equal(0.00m, viewModel.Total);
    }

    [Fact]
    public void ApplyDiscount_RepeatedCalls_OverwritesPreviousDiscountPercentage()
    {
        // Arrange
        var viewModel = new ModernizedCartViewModel();
        viewModel.Items.Add(new CartItem(_croissant, 2)); // $10.00 subtotal

        // Act 1 - Apply 10% discount
        viewModel.ApplyDiscount(10m);
        Assert.Equal(9.00m, viewModel.Total);

        // Act 2 - Re-apply 20% discount (overwrites 10%)
        viewModel.ApplyDiscount(20m);
        Assert.Equal(20m, viewModel.DiscountPercent);
        Assert.Equal(8.00m, viewModel.Total);
    }

    [Fact]
    public void ApplyDiscount_PropertyNotification_FiresPropertyChangedEvents()
    {
        // Arrange
        var viewModel = new ModernizedCartViewModel();
        viewModel.Items.Add(new CartItem(_croissant, 2));

        var notifiedProperties = new List<string>();
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
                notifiedProperties.Add(args.PropertyName);
        };

        // Act
        viewModel.ApplyDiscount(15m);

        // Assert - PropertyChanged fired for DiscountPercent and dependent Total calculation
        Assert.Contains(nameof(ModernizedCartViewModel.DiscountPercent), notifiedProperties);
        Assert.Contains(nameof(ModernizedCartViewModel.Total), notifiedProperties);
    }
}
