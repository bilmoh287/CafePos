# CafePos — .NET MAUI Point-of-Sale System

A modernized, production-ready Point-of-Sale (POS) tablet application built with **.NET 10 MAUI**, **Entity Framework Core**, **SQLite**, and **CommunityToolkit.Mvvm**.

---

## 🚀 Setup & Build Instructions

### Prerequisites
* **.NET 10 SDK** (with MAUI workload installed: `dotnet workload install maui`)
* **Windows 10/11** (Build 19041 or higher)

### Building the Project (Windows Target)
```bash
# Debug Build
dotnet build CafePos.csproj -f net10.0-windows10.0.19041.0

# Release Build
dotnet build CafePos.csproj -c Release -f net10.0-windows10.0.19041.0
```

### Running the Unit Test Suite
```bash
dotnet test tests/CafePos.Tests/CafePos.Tests.csproj -c Release
```

---

## 🖥️ Tested Platform
* **Primary Target**: Windows Desktop / Tablet (`net10.0-windows10.0.19041.0`).

---

## 🏗️ Architecture & Key Features

* **Pattern**: Clean MVVM (`View -> ViewModel -> Service -> EF Core -> SQLite`).
* **Financial Calculations**: Single source of truth for 8.5% sales tax using `decimal` precision and `MidpointRounding.AwayFromZero` (`Helpers/TaxCalculator.cs`).
* **Cart Persistence Safety**: Active cart items are strictly cleared **only after** EF Core SQLite transaction creation succeeds (`_orderService.CreateOrderAsync(...)`).
* **Legacy Xamarin.Forms Migration**: Comprehensive analysis and modernized migration provided in `docs/LEGACY_XAMARIN_ANALYSIS.md` and `Models/Legacy/ModernizedCartViewModel.cs`.

---

## 🌟 Approved Optional Bonus Features

1. **Offline-First Architecture Design Note**: Located at [`docs/OFFLINE_FIRST_SYNC_NOTE.md`](docs/OFFLINE_FIRST_SYNC_NOTE.md). Details outbox pattern queuing, idempotency via client UUIDs, connectivity listeners, and exponential backoff retry strategies.
2. **GitHub Actions CI Pipeline**: Located at [`.github/workflows/ci.yml`](.github/workflows/ci.yml). Automated workflow that restores dependencies, compiles the Windows target, and executes unit tests on push.
3. **Barcode Scan Simulation**: Extensible `IBarcodeScannerService` interface and `MockBarcodeScannerService` implementation with cashier SKU simulation input in the Product Catalog header bar.

---

## ⚠️ Known Limitations

1. **Layout Optimization**: The Product Catalog grid is optimized for wide tablet/desktop windows (3-column grid). On narrow phone screens, the grid items may appear compact.
2. **SQLite Vulnerability Warning (`NU1903`)**: Standard MAUI upstream dependency warning regarding `SQLitePCLRaw.lib.e_sqlite3 2.1.11`. Safe to ignore as it does not impact runtime execution.

---

## 🔮 Next Steps & Future Enhancements

* Replace `MockBarcodeScannerService` with a camera hardware scanner (`ZXing.Net.MAUI`).
* Implement background cloud synchronization engine against an ASP.NET Core Web API backend per [`docs/OFFLINE_FIRST_SYNC_NOTE.md`](docs/OFFLINE_FIRST_SYNC_NOTE.md).

---

## 🤖 AI Disclosure
Assisted by Antigravity AI (Google DeepMind) for code structuring, MVVM refactoring, and test suite implementation.
