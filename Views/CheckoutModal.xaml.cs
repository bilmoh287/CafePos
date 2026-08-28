using CafePos.Models;
using CafePos.ViewModels;

namespace CafePos.Views;

public partial class CheckoutModal : ContentPage
{
    private readonly CheckoutViewModel _viewModel;

    public CheckoutModal(CheckoutViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        _viewModel.OrderCompleted += OnOrderCompleted;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.RefreshState();
    }

    private async void OnOrderCompleted(object? sender, Order order)
    {
        await DisplayAlertAsync(
            "Transaction Successful!",
            $"Order #{order.OrderNumber} saved to SQLite database.\n\n" +
            $"Items: {order.Items.Count}\n" +
            $"Subtotal: ${order.Subtotal:F2}\n" +
            $"Tax (8.5%): ${order.Tax:F2}\n" +
            $"Grand Total: ${order.GrandTotal:F2}\n" +
            $"Payment Method: {order.PaymentMethod}",
            "OK");

        await Navigation.PopModalAsync();
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
