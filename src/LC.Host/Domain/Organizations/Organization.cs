using LC.Host.Domain.Common;
using LC.Host.Domain.Organizations.Events;

namespace LC.Host.Domain.Organizations;

/// <summary>
/// Организация — поставщик курсов (supporting-поддомен).
/// Жизненный цикл: Registered → OnVerification → Verified | VerificationRejected (с повторной подачей).
/// Инвариант «публиковать курсы может только верифицированная организация» проверяет
/// поддомен Courses, беря сюда только флаг <see cref="IsVerified"/> через application-слой.
/// </summary>
public sealed class Organization : AggregateRoot<OrganizationId>
{
    private readonly List<VerificationDocument> _documents = [];

    private Organization()
    {
    }

    private Organization(OrganizationId id, string name, string? description, UserId ownerId)
        : base(id)
    {
        Name = name;
        Description = description;
        OwnerId = ownerId;
        Status = OrganizationStatus.Registered;
        RegisteredAtUtc = DateTime.UtcNow;
    }

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    /// <summary>Пользователь, зарегистрировавший организацию.</summary>
    public UserId OwnerId { get; private set; }

    public OrganizationStatus Status { get; private set; }

    public string? RejectionReason { get; private set; }

    public DateTime RegisteredAtUtc { get; private set; }

    public IReadOnlyList<VerificationDocument> Documents => _documents.AsReadOnly();

    public bool IsVerified => Status == OrganizationStatus.Verified;

    public static Organization Register(string name, string? description, UserId ownerId)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(name), "У организации должно быть название.");

        var organization = new Organization(OrganizationId.New(), name.Trim(), description, ownerId);
        organization.Raise(new OrganizationRegistered(organization.Id, organization.Name));

        return organization;
    }

    /// <summary>Подача документов на верификацию (в том числе повторная после отказа).</summary>
    public void SubmitForVerification(IReadOnlyCollection<(FileId FileId, string Name)> documents)
    {
        DomainException.ThrowIf(
            Status is not (OrganizationStatus.Registered or OrganizationStatus.VerificationRejected),
            "Подать документы на верификацию можно только до её прохождения.");
        DomainException.ThrowIf(documents.Count == 0, "Для верификации нужен хотя бы один документ.");

        _documents.Clear();
        _documents.AddRange(documents.Select(d => new VerificationDocument(d.FileId, d.Name)));

        Status = OrganizationStatus.OnVerification;
        RejectionReason = null;
        Raise(new OrganizationSubmittedForVerification(Id));
    }

    public void ApproveVerification()
    {
        DomainException.ThrowIf(
            Status != OrganizationStatus.OnVerification,
            "Подтвердить верификацию можно только для организации на проверке.");

        Status = OrganizationStatus.Verified;
        Raise(new OrganizationVerified(Id));
    }

    public void RejectVerification(string reason)
    {
        DomainException.ThrowIf(
            Status != OrganizationStatus.OnVerification,
            "Отклонить верификацию можно только для организации на проверке.");
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(reason), "Нужно указать причину отказа.");

        Status = OrganizationStatus.VerificationRejected;
        RejectionReason = reason;
        Raise(new OrganizationVerificationRejected(Id, reason));
    }
}
