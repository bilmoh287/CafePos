using System.Collections.ObjectModel;
using CafePos.Models;
using CafePos.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CafePos.ViewModels;

public partial class CartViewModel : ObservableObject, IDisposable
{
    private readonly ICartService _cartService;

    public ObservableCollection<CartItem> Items { get; } = new();

    [ObservableProperty]
    public partial decimal Subtotal { get; set; }

    [ObservableProperty]
    public partial decimal Tax { get; set; }

    [ObservableProperty]
    public partial decimal GrandTotal { get; set; }

    [ObservableProperty]
    public partial int TotalItemCount { get; set; }

    [ObservableProperty]
    public partial bool IsEmpty { get; set; } = true;

    [ObservableProperty]
    public partial bool HasItems { get; set; }

    public CartViewModel(ICartService cartService)
    {
        _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
        _cartService.CartChanged += OnCartChanged;
        RefreshCartState();
    }

    private void OnCartChanged(object? sender, EventArgs e)
    {
        try
        {
            if (MainThread.IsMainThread)
            {
                RefreshCartState();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(RefreshCartState);
            }
        }
        catch (Exception)
        {
            // Fallback for headless xUnit unit test runners where MAUI DispatcherQueue is not initialized
            RefreshCartState();
        }
    }

    public void RefreshCartState()
    {
        Items.Clear();
        foreach (var item in _cartService.Items)
        {
            Items.Add(item);
        }

        Subtotal = _cartService.Subtotal;
        Tax = _cartService.Tax;
        GrandTotal = _cartService.GrandTotal;
        TotalItemCount = _cartService.TotalItemCount;
        IsEmpty = Items.Count == 0;
        HasItems = Items.Count > 0;
    }

    [RelayCommand]
    public void IncreaseQuantity(CartItem item)
    {
        if (item?.Product == null) return;
        _cartService.AddItem(item.Product, 1);
    }

    [RelayCommand]
    public void DecreaseQuantity(CartItem item)
    {
        if (item?.Product == null) return;
        _cartService.UpdateQuantity(item.Product.Id, item.Quantity - 1);
    }

    [RelayCommand]
    public void RemoveItem(CartItem item)
    {
        if (item?.Product == null) return;
        _cartService.RemoveItem(item.Product.Id);
    }

    [RelayCommand]
    public void ClearCart()
    {
        _cartService.Clear();
    }

    public void Dispose()
    {
        _cartService.CartChanged -= OnCartChanged;
    }
}
