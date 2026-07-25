namespace LC.Host.Domain.Common;

/// <summary>
/// Базовая сущность: идентичность определяется идентификатором, а не значением полей.
/// </summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    protected Entity(TId id) => Id = id;

    /// <summary>Конструктор для материализации ORM.</summary>
    protected Entity() => Id = default!;

    public TId Id { get; protected set; }

    public bool Equals(Entity<TId>? other) =>
        other is not null && other.GetType() == GetType() && EqualityComparer<TId>.Default.Equals(other.Id, Id);

    public override bool Equals(object? obj) => Equals(obj as Entity<TId>);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
