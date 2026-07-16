namespace LC.Host.Domain.Enrollment;

/// <summary>
/// Этапы обработки заявки:
/// получена → на одобрении → согласована / не согласована → одобрена / отклонена.
/// </summary>
public enum ApplicationStatus
{
    /// <summary>Заявка получена.</summary>
    Received = 1,

    /// <summary>На одобрении (взята в работу).</summary>
    UnderReview = 2,

    /// <summary>Согласована — прошла проверку, ждёт финального решения.</summary>
    Agreed = 3,

    /// <summary>Не согласована — проверку не прошла.</summary>
    NotAgreed = 4,

    /// <summary>Одобрена — заявителю выделено место на потоке. Терминальный статус.</summary>
    Approved = 5,

    /// <summary>Отклонена. Терминальный статус.</summary>
    Rejected = 6,
}
