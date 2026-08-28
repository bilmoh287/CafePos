using CafePos.Services.Interfaces;

namespace CafePos.Services.Implementations;

/// <summary>
/// Mock implementation of IBarcodeScannerService that simulates barcode scanning.
/// Returns specified simulated SKU or defaults to SKU-001.
/// </summary>
public class MockBarcodeScannerService : IBarcodeScannerService
{
    public async Task<string> ScanBarcodeAsync(string? defaultSku = null)
    {
        // Simulate hardware scanner read latency
        await Task.Delay(150);

        return string.IsNullOrWhiteSpace(defaultSku) ? "SKU-001" : defaultSku.Trim();
    }
}
