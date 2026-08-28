using System.Collections.ObjectModel;
using CafePos.Models;
using CafePos.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CafePos.ViewModels;

public partial class ProductsViewModel : ObservableObject, IDisposable
{
    private readonly IProductService _productService;
    private readonly ICartService _cartService;
    private readonly IBarcodeScannerService _scannerService;
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

    [ObservableProperty]
    public partial int CartItemCount { get; set; }

    [ObservableProperty]
    public partial decimal CartGrandTotal { get; set; }

    [ObservableProperty]
    public partial bool HasCartItems { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string SimulatedSku { get; set; } = "SKU-001";

    public ProductsViewModel(
        IProductService productService,
        ICartService cartService,
        IBarcodeScannerService scannerService)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
        _scannerService = scannerService ?? throw new ArgumentNullException(nameof(scannerService));

        _cartService.CartChanged += OnCartChanged;
        RefreshCartSummary();
    }

    private void OnCartChanged(object? sender, EventArgs e)
    {
        try
        {
            if (MainThread.IsMainThread)
            {
                RefreshCartSummary();
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(RefreshCartSummary);
            }
        }
        catch
        {
            RefreshCartSummary();
        }
    }

    private void RefreshCartSummary()
    {
        CartItemCount = _cartService.TotalItemCount;
        CartGrandTotal = _cartService.GrandTotal;
        HasCartItems = CartItemCount > 0;
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
        if (product == null) return;
        _cartService.AddItem(product, 1);
        StatusMessage = $"Added {product.Name} to cart";
    }

    [RelayCommand]
    public async Task ScanBarcodeAsync()
    {
        try
        {
            HasError = false;
            ErrorMessage = string.Empty;

            string scannedSku = await _scannerService.ScanBarcodeAsync(SimulatedSku);
            if (string.IsNullOrWhiteSpace(scannedSku)) return;

            var matches = await _productService.GetProductsAsync(scannedSku);
            var scannedProduct = matches.FirstOrDefault(p => string.Equals(p.SKU, scannedSku, StringComparison.OrdinalIgnoreCase));

            if (scannedProduct != null)
            {
                _cartService.AddItem(scannedProduct, 1);
                StatusMessage = $"📷 Scanned {scannedProduct.Name} ({scannedSku})";
            }
            else
            {
                StatusMessage = $"⚠️ Product with SKU '{scannedSku}' not found.";
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Barcode scan failed: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task OpenCartAsync()
    {
        await Shell.Current.GoToAsync("//CartPage");
    }

    public void Dispose()
    {
        _cartService.CartChanged -= OnCartChanged;
    }
}
