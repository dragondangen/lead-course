namespace LC.Host.Domain.Common;

/// <summary>
/// Идентификатор пользователя. Пользователи — generic-поддомен Identity;
/// остальные поддомены ссылаются на них только по идентификатору.
/// </summary>
public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}

/// <summary>
/// Идентификатор файла в хранилище (видео, PDF, документы верификации).
/// Само хранилище — инфраструктурная забота (generic-поддомен Files).
/// </summary>
public readonly record struct FileId(Guid Value)
{
    public static FileId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
