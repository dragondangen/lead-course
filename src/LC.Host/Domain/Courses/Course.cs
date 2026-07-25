using LC.Host.Domain.Common;
using LC.Host.Domain.Courses.Events;
using LC.Host.Domain.Organizations;

namespace LC.Host.Domain.Courses;

/// <summary>
/// Курс организации (core-поддомен). Жизненный цикл: Draft → Published → Archived.
/// Кросс-агрегатные правила («публиковать может только верифицированная организация»,
/// «архивировать нельзя при активных потоках») application-слой вычисляет заранее
/// и передаёт сюда готовым флагом — сам агрегат чужие агрегаты не загружает.
/// </summary>
public sealed class Course : AggregateRoot<CourseId>
{
    private Course()
    {
    }

    private Course(CourseId id, OrganizationId organizationId, string title, string? description, Price price)
        : base(id)
    {
        OrganizationId = organizationId;
        Title = title;
        Description = description;
        Price = price;
        Status = CourseStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public OrganizationId OrganizationId { get; private set; }

    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public Price Price { get; private set; } = null!;

    public CourseStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static Course CreateDraft(OrganizationId organizationId, string title, string? description, Price price)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(title), "У курса должно быть название.");

        var course = new Course(CourseId.New(), organizationId, title.Trim(), description, price);
        course.Raise(new CourseCreated(course.Id, organizationId));

        return course;
    }

    public void UpdateDetails(string title, string? description, Price price)
    {
        EnsureNotArchived();
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(title), "У курса должно быть название.");

        Title = title.Trim();
        Description = description;
        Price = price;
    }

    /// <param name="organizationIsVerified">
    /// Флаг верификации организации (<see cref="Organization.IsVerified"/>), полученный application-слоем.
    /// </param>
    /// <param name="hasAtLeastOneMaterial">
    /// Признак наличия хотя бы одного материала. Материалы — отдельные агрегаты
    /// (<see cref="CourseMaterial"/>), поэтому проверку делает application-слой запросом-счётчиком,
    /// а не загрузкой всех материалов в курс.
    /// </param>
    public void Publish(bool organizationIsVerified, bool hasAtLeastOneMaterial)
    {
        DomainException.ThrowIf(Status != CourseStatus.Draft, "Опубликовать можно только черновик курса.");
        DomainException.ThrowIf(!organizationIsVerified, "Публиковать курсы может только верифицированная организация.");
        DomainException.ThrowIf(!hasAtLeastOneMaterial, "Нельзя опубликовать курс без учебных материалов.");

        Status = CourseStatus.Published;
        Raise(new CoursePublished(Id));
    }

    /// <param name="hasActiveCohorts">
    /// Признак наличия незавершённых потоков; вычисляется application-слоем.
    /// </param>
    public void Archive(bool hasActiveCohorts)
    {
        EnsureNotArchived();
        DomainException.ThrowIf(hasActiveCohorts, "Нельзя архивировать курс с активными потоками.");

        Status = CourseStatus.Archived;
        Raise(new CourseArchived(Id));
    }

    private void EnsureNotArchived() =>
        DomainException.ThrowIf(Status == CourseStatus.Archived, "Курс в архиве изменять нельзя.");
}
