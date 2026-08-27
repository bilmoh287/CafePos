using System.Collections.ObjectModel;
using CafePos.Models;
using CafePos.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CafePos.ViewModels;

public partial class ProductsViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private CancellationTokenSource? _searchCts;

    public ObservableCollection<Product> Products { get; } = new();
    public ObservableCollection<string> Categories { get; } = new();

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SelectedCategory { get; set; } = "All";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotLoading))]
    public partial bool IsLoading { get; set; }

    public bool IsNotLoading => !IsLoading;

    [ObservableProperty]
    public partial bool IsEmpty { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    public ProductsViewModel(IProductService productService)
    {
        _productService = productService;
    }

    partial void OnSearchQueryChanged(string value)
    {
        _searchCts?.Cancel();
        _searchCts?.Dispose();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        _ = SearchDebouncedAsync(token);
    }

    private async Task SearchDebouncedAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(300, token);
            await LoadProductsAsync(token);
        }
        catch (OperationCanceledException)
        {
            // Expected when cashier types another character within 300ms
        }
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        await LoadProductsAsync();
    }

    [RelayCommand]
    public async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await _productService.GetCategoriesAsync();
            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Failed to load categories: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task SelectCategoryAsync(string category)
    {
        if (string.Equals(SelectedCategory, category, StringComparison.OrdinalIgnoreCase))
            return;

        SelectedCategory = category;
        await LoadProductsAsync();
    }

    [RelayCommand]
    public async Task LoadProductsAsync(CancellationToken token = default)
    {
        try
        {
            IsLoading = true;
            IsEmpty = false;
            HasError = false;
            ErrorMessage = string.Empty;

            var items = await _productService.GetProductsAsync(SearchQuery, SelectedCategory);

            token.ThrowIfCancellationRequested();

            Products.Clear();
            foreach (var item in items)
            {
                Products.Add(item);
            }

            IsEmpty = Products.Count == 0;
        }
        catch (OperationCanceledException)
        {
            // Search request was cancelled by a newer query
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Failed to load catalog: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void AddToCart(Product product)
    {
        // Command signature ready for ICartService integration in Milestone 6.
        // Intentionally no fake cart state or false popups created prior to Milestone 6.
        if (product == null) return;
    }
}
