namespace LC.Host.Domain.Common;

/// <summary>
/// Период дат (например, поток «июнь — август»). Объект-значение.
/// </summary>
public sealed record DateRange
{
    private DateRange(DateOnly start, DateOnly end)
    {
        Start = start;
        End = end;
    }

    public DateOnly Start { get; }

    public DateOnly End { get; }

    public static DateRange Create(DateOnly start, DateOnly end)
    {
        DomainException.ThrowIf(end <= start, "Дата окончания периода должна быть позже даты начала.");

        return new DateRange(start, end);
    }
}
