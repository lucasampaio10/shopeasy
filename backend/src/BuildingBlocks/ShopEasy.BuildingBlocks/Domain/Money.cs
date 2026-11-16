namespace ShopEasy.BuildingBlocks.Domain;

/// <summary>Monetary amount in a given currency. Immutable and compared by value.</summary>
public sealed record Money
{
    public const string DefaultCurrency = "BRL";

    public Money(decimal amount, string currency = DefaultCurrency)
    {
        if (amount < 0)
        {
            throw new DomainException("money.negative_amount", "A monetary amount cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
        {
            throw new DomainException("money.invalid_currency", "Currency must be a three-letter ISO 4217 code.");
        }

        Amount = decimal.Round(amount, 2, MidpointRounding.ToEven);
        Currency = currency.Trim().ToUpperInvariant();
    }

    private Money()
    {
    }

    public decimal Amount { get; private init; }

    public string Currency { get; private init; } = DefaultCurrency;

    public static Money Zero(string currency = DefaultCurrency) => new(0m, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Multiply(int factor) => new(Amount * factor, Currency);

    public override string ToString() => $"{Currency} {Amount:0.00}";

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new DomainException("money.currency_mismatch", $"Cannot combine {Currency} with {other.Currency}.");
        }
    }
}
