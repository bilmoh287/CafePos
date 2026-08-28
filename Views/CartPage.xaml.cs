using CafePos.ViewModels;

namespace CafePos.Views;

public partial class CartPage : ContentPage
{
    private readonly CheckoutModal _checkoutModal;

    public CartPage(CartViewModel viewModel, CheckoutModal checkoutModal)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _checkoutModal = checkoutModal ?? throw new ArgumentNullException(nameof(checkoutModal));
    }

    private async void OnProceedToPaymentClicked(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(_checkoutModal);
    }
}
