namespace LC.Host.Domain.Enrollment;

public readonly record struct ApplicationId(Guid Value)
{
    public static ApplicationId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
