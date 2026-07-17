using LC.Host.Domain.Common;

namespace LC.Host.Domain.Courses;

/// <summary>
/// Цена курса: бесплатный или платный с конкретной суммой.
/// </summary>
public sealed record Price
{
    private Price(Money? value) => Value = value;

    /// <summary>Сумма; null для бесплатного курса.</summary>
    public Money? Value { get; }

    public bool IsFree => Value is null;

    public static Price Free() => new((Money?)null);

    public static Price Paid(Money value)
    {
        DomainException.ThrowIf(value.Amount == 0, "Цена платного курса должна быть больше нуля.");

        return new Price(value);
    }
}
