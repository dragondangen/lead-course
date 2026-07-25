using LC.Host.Domain.Common;

namespace LC.Host.Domain.Notifications;

public enum NotificationChannel
{
    Email = 1,
    Push = 2,
    InApp = 3,
}

public enum NotificationStatus
{
    Pending = 1,
    Sent = 2,
    Failed = 3,
}

/// <summary>
/// Уведомление пользователю (generic-поддомен). Модуль подписан на доменные события
/// остальных поддоменов (заявка одобрена, поток отменён и т.д.) и превращает их
/// в отправку по каналам. Ретраи и провайдеры доставки — забота инфраструктуры.
/// </summary>
public sealed class Notification : AggregateRoot<NotificationId>
{
    private Notification()
    {
    }

    private Notification(NotificationId id, UserId recipientId, NotificationChannel channel, string subject, string body)
        : base(id)
    {
        RecipientId = recipientId;
        Channel = channel;
        Subject = subject;
        Body = body;
        Status = NotificationStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public UserId RecipientId { get; private set; }

    public NotificationChannel Channel { get; private set; }

    public string Subject { get; private set; } = null!;

    public string Body { get; private set; } = null!;

    public NotificationStatus Status { get; private set; }

    public int AttemptCount { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? SentAtUtc { get; private set; }

    public string? FailureReason { get; private set; }

    public static Notification Create(UserId recipientId, NotificationChannel channel, string subject, string body)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(subject), "У уведомления должна быть тема.");
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(body), "У уведомления должен быть текст.");

        return new Notification(NotificationId.New(), recipientId, channel, subject, body);
    }

    public void MarkSent()
    {
        DomainException.ThrowIf(Status == NotificationStatus.Sent, "Уведомление уже отправлено.");

        Status = NotificationStatus.Sent;
        SentAtUtc = DateTime.UtcNow;
        AttemptCount++;
    }

    public void MarkFailed(string reason)
    {
        DomainException.ThrowIf(Status == NotificationStatus.Sent, "Отправленное уведомление нельзя пометить ошибочным.");

        Status = NotificationStatus.Failed;
        FailureReason = reason;
        AttemptCount++;
    }
}
