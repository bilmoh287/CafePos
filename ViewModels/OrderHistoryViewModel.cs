using System.Collections.ObjectModel;
using CafePos.Models;
using CafePos.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CafePos.ViewModels;

public partial class OrderHistoryViewModel : ObservableObject
{
    private readonly IOrderService _orderService;

    public ObservableCollection<Order> Orders { get; } = new();

    [ObservableProperty]
    public partial Order? SelectedOrder { get; set; }

    [ObservableProperty]
    public partial bool IsShowingReceipt { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool IsEmpty { get; set; } = true;

    [ObservableProperty]
    public partial bool HasOrders { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    public OrderHistoryViewModel(IOrderService orderService)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
    }

    [RelayCommand]
    public async Task LoadOrdersAsync()
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            var ordersList = await _orderService.GetOrdersAsync();

            Orders.Clear();
            foreach (var order in ordersList)
            {
                Orders.Add(order);
            }

            IsEmpty = Orders.Count == 0;
            HasOrders = Orders.Count > 0;
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Failed to load order history: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void SelectOrder(Order order)
    {
        if (order == null) return;
        SelectedOrder = order;
        IsShowingReceipt = true;
    }

    [RelayCommand]
    public void CloseReceipt()
    {
        IsShowingReceipt = false;
        SelectedOrder = null;
    }
}
