using CafePos.Models;
using CafePos.Services.Implementations;
using CafePos.ViewModels;
using Xunit;

namespace CafePos.Tests;

public class CartServiceTests
{
    private readonly Product _espresso = new Product { Id = 1, SKU = "SKU-001", Name = "Espresso", Price = 2.50m, Category = "Beverages" };
    private readonly Product _croissant = new Product { Id = 2, SKU = "SKU-002", Name = "Croissant", Price = 3.25m, Category = "Bakery" };
    private readonly Product _sandwich = new Product { Id = 6, SKU = "SKU-006", Name = "Turkey Sandwich", Price = 6.95m, Category = "Food" };

    [Fact]
    public void InitialState_IsEmpty()
    {
        // Arrange & Act
        var cart = new CartService();

        // Assert
        Assert.Empty(cart.Items);
        Assert.Equal(0m, cart.Subtotal);
        Assert.Equal(0m, cart.Tax);
        Assert.Equal(0m, cart.GrandTotal);
        Assert.Equal(0, cart.TotalItemCount);
    }

    [Fact]
    public void AddItem_SingleProduct_CreatesSingleLineItem()
    {
        // Arrange
        var cart = new CartService();

        // Act
        cart.AddItem(_espresso, 1);

        // Assert
        Assert.Single(cart.Items);
        Assert.Equal(_espresso.Id, cart.Items[0].Product.Id);
        Assert.Equal(1, cart.Items[0].Quantity);
        Assert.Equal(2.50m, cart.Items[0].TotalPrice);
        Assert.Equal(2.50m, cart.Subtotal);
        Assert.Equal(0.21m, cart.Tax); // $2.50 * 0.085 = 0.2125 -> $0.21
        Assert.Equal(2.71m, cart.GrandTotal); // $2.50 + $0.21 = $2.71
    }

    [Fact]
    public void AddItem_SameProductTwice_IncrementsQuantity()
    {
        // Arrange
        var cart = new CartService();

        // Act
        cart.AddItem(_espresso, 1);
        cart.AddItem(_espresso, 1);

        // Assert
        Assert.Single(cart.Items);
        Assert.Equal(2, cart.Items[0].Quantity);
        Assert.Equal(5.00m, cart.Items[0].TotalPrice);
        Assert.Equal(5.00m, cart.Subtotal);
        Assert.Equal(0.43m, cart.Tax); // $5.00 * 0.085 = 0.425 -> $0.43
        Assert.Equal(5.43m, cart.GrandTotal);
    }

    [Fact]
    public void AddItem_DifferentProducts_CreatesDistinctLineItems()
    {
        // Arrange
        var cart = new CartService();

        // Act
        cart.AddItem(_espresso, 2);  // $5.00
        cart.AddItem(_croissant, 1); // $3.25

        // Assert
        Assert.Equal(2, cart.Items.Count);
        Assert.Equal(3, cart.TotalItemCount);
        Assert.Equal(8.25m, cart.Subtotal); // $5.00 + $3.25 = $8.25
        Assert.Equal(0.70m, cart.Tax);      // $8.25 * 0.085 = 0.70125 -> $0.70
        Assert.Equal(8.95m, cart.GrandTotal); // $8.25 + $0.70 = $8.95
    }

    [Fact]
    public void UpdateQuantity_DecrementQuantity_UpdatesTotals()
    {
        // Arrange
        var cart = new CartService();
        cart.AddItem(_espresso, 3);

        // Act
        cart.UpdateQuantity(_espresso.Id, 2);

        // Assert
        Assert.Single(cart.Items);
        Assert.Equal(2, cart.Items[0].Quantity);
        Assert.Equal(5.00m, cart.Subtotal);
    }

    [Fact]
    public void UpdateQuantity_ZeroOrNegativeQuantity_RemovesItem()
    {
        // Arrange
        var cart = new CartService();
        cart.AddItem(_espresso, 1);

        // Act
        cart.UpdateQuantity(_espresso.Id, 0);

        // Assert
        Assert.Empty(cart.Items);
        Assert.Equal(0m, cart.Subtotal);
        Assert.Equal(0m, cart.Tax);
        Assert.Equal(0m, cart.GrandTotal);
    }

    [Fact]
    public void RemoveItem_ExistingProduct_RemovesFromCart()
    {
        // Arrange
        var cart = new CartService();
        cart.AddItem(_espresso, 2);
        cart.AddItem(_croissant, 1);

        // Act
        cart.RemoveItem(_espresso.Id);

        // Assert
        Assert.Single(cart.Items);
        Assert.Equal(_croissant.Id, cart.Items[0].Product.Id);
        Assert.Equal(3.25m, cart.Subtotal);
    }

    [Fact]
    public void Clear_ResetsAllItemsAndTotalsToZero()
    {
        // Arrange
        var cart = new CartService();
        cart.AddItem(_espresso, 2);
        cart.AddItem(_sandwich, 1);

        // Act
        cart.Clear();

        // Assert
        Assert.Empty(cart.Items);
        Assert.Equal(0m, cart.Subtotal);
        Assert.Equal(0m, cart.Tax);
        Assert.Equal(0m, cart.GrandTotal);
        Assert.Equal(0, cart.TotalItemCount);
    }

    [Fact]
    public void CartViewModel_UpdatesOnSharedCartServiceChange()
    {
        // Arrange
        var cartService = new CartService();
        using var viewModel = new CartViewModel(cartService);

        // Act
        cartService.AddItem(_espresso, 2); // $5.00 subtotal

        // Assert
        Assert.Single(viewModel.Items);
        Assert.Equal(2, viewModel.TotalItemCount);
        Assert.Equal(5.00m, viewModel.Subtotal);
        Assert.Equal(0.43m, viewModel.Tax);
        Assert.Equal(5.43m, viewModel.GrandTotal);
        Assert.False(viewModel.IsEmpty);

        // Act - Clear cart
        cartService.Clear();

        // Assert
        Assert.Empty(viewModel.Items);
        Assert.Equal(0m, viewModel.Subtotal);
        Assert.True(viewModel.IsEmpty);
    }
}
