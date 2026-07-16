using LC.Host.Domain.Common;

namespace LC.Host.Domain.Courses;

public readonly record struct CourseMaterialID(Guid Value)
{
    public static CourseMaterialID New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}

public enum MaterialType
{
    Video = 1,
    Pdf = 2,
    Presentation = 3,
    Archive = 4,
    Link = 5,
}

/// <summary>
/// Учебный материал курса (видео, PDF и т.д.). Часть агрегата <see cref="Course"/>.
/// Файл хранится в файловом хранилище, здесь — только ссылка (<see cref="FileId"/>).
/// </summary>
public sealed class CourseMaterial : Entity<CourseMaterialID>
{
    private CourseMaterial()
    {
    }

    internal CourseMaterial(string title, MaterialType type, FileId fileId, int sortOrder)
        : base(CourseMaterialID.New())
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(title), "У материала должно быть название.");

        Title = title;
        Type = type;
        FileId = fileId;
        SortOrder = sortOrder;
    }

    public string Title { get; private set; } = null!;

    public MaterialType Type { get; private set; }

    public FileId FileId { get; private set; }

    public int SortOrder { get; private set; }
}
