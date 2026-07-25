using LC.Host.Domain.Common;
using LC.Host.Domain.Courses.Events;

namespace LC.Host.Domain.Courses;

public readonly record struct CourseMaterialId(Guid Value)
{
    public static CourseMaterialId New() => new(Guid.NewGuid());

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
/// Учебный материал курса (видео, PDF и т.д.) — самостоятельный агрегат, а НЕ часть Course.
/// Так корень курса остаётся «лёгким»: у популярного курса материалов могут быть сотни,
/// и грузить их все ради правки названия курса недопустимо. На курс ссылается по <see cref="CourseId"/>.
/// Файл лежит в файловом хранилище, здесь — только ссылка (<see cref="FileId"/>).
/// </summary>
public sealed class CourseMaterial : AggregateRoot<CourseMaterialId>
{
    private CourseMaterial()
    {
    }

    private CourseMaterial(
        CourseMaterialId id,
        CourseId courseId,
        string title,
        MaterialType type,
        FileId fileId,
        int sortOrder)
        : base(id)
    {
        CourseId = courseId;
        Title = title;
        Type = type;
        FileId = fileId;
        SortOrder = sortOrder;
    }

    public CourseId CourseId { get; private set; }

    public string Title { get; private set; } = null!;

    public MaterialType Type { get; private set; }

    public FileId FileId { get; private set; }

    /// <summary>Порядковый номер в списке материалов курса.</summary>
    public int SortOrder { get; private set; }

    /// <param name="courseIsArchived">
    /// Статус курса (<see cref="Course.Status"/> == Archived); загружается application-слоем.
    /// </param>
    /// <param name="sortOrder">
    /// Позиция материала; application-слой вычисляет её (например, max+1) без загрузки всего курса.
    /// </param>
    public static CourseMaterial Add(
        CourseId courseId,
        bool courseIsArchived,
        string title,
        MaterialType type,
        FileId fileId,
        int sortOrder)
    {
        DomainException.ThrowIf(courseIsArchived, "В архивный курс нельзя добавлять материалы.");
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(title), "У материала должно быть название.");
        DomainException.ThrowIf(sortOrder < 1, "Позиция материала должна быть положительной.");

        var material = new CourseMaterial(CourseMaterialId.New(), courseId, title.Trim(), type, fileId, sortOrder);
        material.Raise(new CourseMaterialAdded(material.Id, courseId));

        return material;
    }

    public void Rename(string title)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(title), "У материала должно быть название.");

        Title = title.Trim();
    }

    public void MoveTo(int sortOrder)
    {
        DomainException.ThrowIf(sortOrder < 1, "Позиция материала должна быть положительной.");

        SortOrder = sortOrder;
    }
}
