namespace CafePos.Services.Interfaces;

/// <summary>
/// Abstraction representing a hardware or camera barcode scanner device.
/// Decouples UI and ViewModel from scanner hardware implementation details.
/// </summary>
public interface IBarcodeScannerService
{
    Task<string> ScanBarcodeAsync(string? defaultSku = null);
}
