using CafePos.Models;
using CafePos.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CafePos.ViewModels;

public partial class CheckoutViewModel : ObservableObject, IDisposable
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;

    [ObservableProperty]
    public partial PaymentMethod SelectedPaymentMethod { get; set; } = PaymentMethod.Cash;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCanPay))]
    public partial bool IsProcessing { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    public decimal Subtotal => _cartService.Subtotal;
    public decimal Tax => _cartService.Tax;
    public decimal GrandTotal => _cartService.GrandTotal;
    public int TotalItemCount => _cartService.TotalItemCount;
    public IReadOnlyList<CartItem> Items => _cartService.Items;

    public bool IsCanPay => TotalItemCount > 0 && !IsProcessing;

    public event EventHandler<Order>? OrderCompleted;

    public CheckoutViewModel(ICartService cartService, IOrderService orderService)
    {
        _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        
        _cartService.CartChanged += OnCartChanged;
    }

    private void OnCartChanged(object? sender, EventArgs e)
    {
        RefreshState();
    }

    public void RefreshState()
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(Tax));
        OnPropertyChanged(nameof(GrandTotal));
        OnPropertyChanged(nameof(TotalItemCount));
        OnPropertyChanged(nameof(Items));
        OnPropertyChanged(nameof(IsCanPay));
    }

    [RelayCommand]
    public void SelectPaymentMethod(PaymentMethod method)
    {
        SelectedPaymentMethod = method;
    }

    [RelayCommand]
    public async Task ProcessCheckoutAsync()
    {
        if (_cartService.Items.Count == 0 || TotalItemCount == 0)
        {
            HasError = true;
            ErrorMessage = "Cart is empty. Please add items before completing payment.";
            return;
        }

        if (IsProcessing) return;

        try
        {
            IsProcessing = true;
            HasError = false;
            ErrorMessage = string.Empty;

            // 1. Create & Persist Order in EF Core Database
            var completedOrder = await _orderService.CreateOrderAsync(_cartService.Items, SelectedPaymentMethod);

            // 2. Clear cart ONLY AFTER successful database persistence confirmation
            _cartService.Clear();

            // 3. Notify subscribers of successful completed order
            OrderCompleted?.Invoke(this, completedOrder);
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Unable to complete transaction: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    public void Dispose()
    {
        _cartService.CartChanged -= OnCartChanged;
    }
}
