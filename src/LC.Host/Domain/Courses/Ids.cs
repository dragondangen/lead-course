namespace LC.Host.Domain.Courses;

public readonly record struct CourseId(Guid Value)
{
    public static CourseId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}

public readonly record struct CohortId(Guid Value)
{
    public static CohortId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
