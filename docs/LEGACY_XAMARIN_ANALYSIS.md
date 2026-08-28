# Legacy Xamarin.Forms Code Snippet Analysis & .NET MAUI Port

This document satisfies **Section 4 of the Technical Assessment Specification**: analyzing bugs and architectural defects in the legacy Xamarin.Forms `CartViewModel` snippet and explaining the modernized .NET MAUI implementation.

---

## 1. Assessment-Hinted Core Issues

### Issue A: Percentage Scale Factor / Unit Bug
* **Snippet Code**:
  ```csharp
  total = total - (total * DiscountPercent);
  ```
* **Analysis**: The code assumes `DiscountPercent` is stored as a decimal fraction (e.g. `0.20` for 20%). However, `ApplyDiscount(double percent)` passes whole percentage numbers directly (e.g. `20`).
* **Impact**: Passing `20` intending a 20% discount results in `total - (total * 20.0)`, which subtracts **2000%** of the total, producing a severe negative total (`-19 × total`).
* **Fix**: Convert percentage input by dividing by `100` (i.e. `DiscountPercent = percent / 100.0`) or calculate `total - (total * (DiscountPercent / 100.0))`.

### Issue B: Broken MVVM & Property Change Notification Behavior
* **Snippet Code**:
  ```csharp
  public class CartViewModel : INotifyPropertyChanged
  ```
* **Analysis**:
  1. The class declares interface compliance `: INotifyPropertyChanged`, but **never declares or raises** the `PropertyChangedEventHandler? PropertyChanged;` event.
  2. `Items` is declared as a plain `List<CartItem>`, which does not emit collection change notifications when items are added or removed.
* **Impact**: XAML data bindings fail silently. When a cashier applies a discount or modifies items, the UI view never updates or re-renders the total.
* **Fix**: Inherit `ObservableObject` from `CommunityToolkit.Mvvm`, use `ObservableCollection<T>`, and decorate properties with `[ObservableProperty]` and `[NotifyPropertyChangedFor(nameof(Total))]`.

### Issue C: Repeated `ApplyDiscount` Calls Behavior
* **Snippet Code**:
  ```csharp
  public void ApplyDiscount(double percent)
  {
      DiscountPercent = percent;
  }
  ```
* **Analysis**: Calling `ApplyDiscount` assigns `DiscountPercent` directly rather than accumulating or compounding discounts.
* **Intended POS Behavior Assumption**: Direct assignment is the correct business intent for a cashier overriding or applying a single order discount (e.g. applying a 10% discount, then updating to 20% should result in a 20% total discount, not a 30% or compounded discount). However, the snippet lacked validation against invalid input (negative discounts or discounts > 100%).
* **Fix**: Validate and clamp `percent` between `0` and `100` upon assignment. Calling `ApplyDiscount` repeatedly cleanly updates the active discount percentage.

---

## 2. Additional Engineering Improvements

### Improvement D: Currency Precision (`double` vs `decimal`)
* **Analysis**: The legacy code used `double` for `Price` and `total`. Binary floating-point numbers cannot accurately represent base-10 decimals, leading to floating-point rounding errors (e.g. `$0.10 + $0.20 = $0.30000000000000004`).
* **Fix**: All monetary calculations in .NET MAUI use `decimal` with explicit `MidpointRounding.AwayFromZero` rounding.

---

## 3. Modernized .NET MAUI Implementation

The legacy code has been refactored in `Models/Legacy/ModernizedCartViewModel.cs` using `CommunityToolkit.Mvvm` patterns, `decimal` monetary types, and proper input clamping.
