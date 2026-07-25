using LC.Host.Domain.Common;

namespace LC.Host.Domain.Organizations;

/// <summary>
/// Документ, загруженный организацией для верификации.
/// Часть агрегата <see cref="Organization"/>, извне недоступен.
/// </summary>
public sealed class VerificationDocument : Entity<Guid>
{
    private VerificationDocument()
    {
    }

    internal VerificationDocument(FileId fileId, string name)
        : base(Guid.NewGuid())
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(name), "У документа должно быть название.");

        FileId = fileId;
        Name = name;
        UploadedAtUtc = DateTime.UtcNow;
    }

    public FileId FileId { get; private set; }

    public string Name { get; private set; } = null!;

    public DateTime UploadedAtUtc { get; private set; }
}
