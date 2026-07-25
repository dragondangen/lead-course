namespace LC.Host.Domain.Common;

/// <summary>
/// Доменное событие — факт, произошедший внутри агрегата.
/// Публикуется после успешного сохранения агрегата (transactional outbox).
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }

    DateTime OccurredAtUtc { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();

    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}
