namespace CafePos.Helpers;

public static class TaxCalculator
{
    public const decimal TaxRate = 0.085m; // 8.5% Sales Tax as specified

    /// <summary>
    /// Calculates the 8.5% sales tax for a given subtotal, rounded to 2 decimal places.
    /// </summary>
    public static decimal CalculateTax(decimal subtotal)
    {
        if (subtotal <= 0m)
            return 0m;

        return Math.Round(subtotal * TaxRate, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Computes the grand total by summing subtotal and tax.
    /// </summary>
    public static decimal CalculateGrandTotal(decimal subtotal, decimal tax)
    {
        return subtotal + tax;
    }
}
