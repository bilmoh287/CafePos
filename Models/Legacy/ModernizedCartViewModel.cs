using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CafePos.Models.Legacy;

/// <summary>
/// Modernized .NET MAUI port of the legacy Xamarin.Forms CartViewModel snippet (Section 7).
/// Demonstrates fixes for percentage scale factor, INotifyPropertyChanged MVVM bindings,
/// ObservableCollection usage, decimal currency precision, and clamped discount handling.
/// </summary>
public partial class ModernizedCartViewModel : ObservableObject
{
    public ObservableCollection<CartItem> Items { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Subtotal))]
    [NotifyPropertyChangedFor(nameof(DiscountAmount))]
    [NotifyPropertyChangedFor(nameof(Total))]
    public partial decimal DiscountPercent { get; set; }

    public decimal Subtotal => Items.Sum(item => item.TotalPrice);

    public decimal DiscountAmount => Math.Round(Subtotal * (DiscountPercent / 100m), 2, MidpointRounding.AwayFromZero);

    public decimal Total => Math.Max(0m, Subtotal - DiscountAmount);

    [RelayCommand]
    public void ApplyDiscount(decimal percent)
    {
        // Clamps percentage input between 0% and 100%
        // Replaces active discount percentage (intended override behavior)
        DiscountPercent = Math.Clamp(percent, 0m, 100m);
    }
}
