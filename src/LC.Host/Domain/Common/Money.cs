namespace LC.Host.Domain.Common;

/// <summary>
/// Денежная сумма. Неизменяемый объект-значение.
/// </summary>
public sealed record Money
{
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }

    /// <summary>Код валюты по ISO 4217, например "RUB".</summary>
    public string Currency { get; }

    public static Money Create(decimal amount, string currency)
    {
        DomainException.ThrowIf(amount < 0, "Денежная сумма не может быть отрицательной.");
        DomainException.ThrowIf(
            string.IsNullOrWhiteSpace(currency) || currency.Length != 3,
            "Валюта должна быть трёхбуквенным кодом ISO 4217.");

        return new Money(amount, currency.ToUpperInvariant());
    }
}
