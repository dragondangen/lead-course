using LC.Host.Domain.Common;
using LC.Host.Domain.Courses;
using LC.Host.Domain.Enrollment.Events;

namespace LC.Host.Domain.Enrollment;

/// <summary>
/// Заявка пользователя на поток курса (core-поддомен).
/// Агрегат сознательно маленький (без коллекций и ссылок на другие агрегаты):
/// при 300 000 заявок на курс это критично для конкурентной поэтапной обработки.
/// Место на потоке занимается только в момент одобрения — обработчик
/// <see cref="Events.ApplicationApproved"/> вызывает Cohort.ReserveSeat() в поддомене Courses;
/// если мест не осталось, одобрение откатывается.
/// </summary>
public sealed class EnrollmentApplication : AggregateRoot<ApplicationId>
{
    /// <summary>
    /// Карта допустимых переходов между этапами. Новый этап — одна строка здесь
    /// плюс метод-команда, существующие переходы не трогаем (принцип открытости/закрытости).
    /// </summary>
    private static readonly IReadOnlyDictionary<ApplicationStatus, ApplicationStatus[]> AllowedTransitions =
        new Dictionary<ApplicationStatus, ApplicationStatus[]>
        {
            [ApplicationStatus.Received] = [ApplicationStatus.UnderReview, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn],
            [ApplicationStatus.UnderReview] = [ApplicationStatus.Agreed, ApplicationStatus.NotAgreed, ApplicationStatus.Withdrawn],
            [ApplicationStatus.Agreed] = [ApplicationStatus.Approved, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn],
            [ApplicationStatus.Approved] = [ApplicationStatus.Withdrawn],
            [ApplicationStatus.NotAgreed] = [ApplicationStatus.Rejected],
        };

    private EnrollmentApplication()
    {
    }

    private EnrollmentApplication(ApplicationId id, CohortId cohortId, UserId applicantId, string? motivation)
        : base(id)
    {
        CohortId = cohortId;
        ApplicantId = applicantId;
        Motivation = motivation;
        Status = ApplicationStatus.Received;
        SubmittedAtUtc = DateTime.UtcNow;
        StatusChangedAtUtc = SubmittedAtUtc;
    }

    public CohortId CohortId { get; private set; }

    public UserId ApplicantId { get; private set; }

    /// <summary>Сопроводительный текст заявителя, если курс его требует.</summary>
    public string? Motivation { get; private set; }

    public ApplicationStatus Status { get; private set; }

    /// <summary>Комментарий модератора к последнему решению.</summary>
    public string? DecisionComment { get; private set; }

    public DateTime SubmittedAtUtc { get; private set; }

    public DateTime StatusChangedAtUtc { get; private set; }

    /// <param name="enrollmentIsOpen">
    /// Статус приёма на поток (<see cref="Cohort.Status"/> == EnrollmentOpen); application-слой берёт его из Courses.
    /// </param>
    /// <param name="applicantAlreadyApplied">
    /// Результат проверки «у пользователя уже есть активная заявка на этот поток».
    /// </param>
    public static EnrollmentApplication Submit(
        CohortId cohortId,
        UserId applicantId,
        string? motivation,
        bool enrollmentIsOpen,
        bool applicantAlreadyApplied)
    {
        DomainException.ThrowIf(!enrollmentIsOpen, "Приём заявок на этот поток не открыт.");
        DomainException.ThrowIf(applicantAlreadyApplied, "У пользователя уже есть заявка на этот поток.");

        var application = new EnrollmentApplication(ApplicationId.New(), cohortId, applicantId, motivation);
        application.Raise(new ApplicationSubmitted(application.Id, cohortId, applicantId));

        return application;
    }

    public void TakeInReview()
    {
        MoveTo(ApplicationStatus.UnderReview);
        Raise(new ApplicationTakenInReview(Id, ApplicantId));
    }

    public void Withdraw(bool cohortHasStarted)
    {
        DomainException.ThrowIf(cohortHasStarted, "После начала обучения заявку отозвать нельзя.");

        var seatWasReserved = Status == ApplicationStatus.Approved; // До перехода

        MoveTo(ApplicationStatus.Withdrawn);
        Raise(new ApplicationWithdrawn(Id, CohortId, ApplicantId, seatWasReserved));
    }

    public void Agree()
    {
        MoveTo(ApplicationStatus.Agreed);
        Raise(new ApplicationAgreed(Id, ApplicantId));
    }

    public void Disagree(string reason)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(reason), "Нужно указать причину несогласования.");

        MoveTo(ApplicationStatus.NotAgreed);
        DecisionComment = reason;
        Raise(new ApplicationNotAgreed(Id, ApplicantId, reason));
    }

    public void Approve()
    {
        MoveTo(ApplicationStatus.Approved);
        Raise(new ApplicationApproved(Id, CohortId, ApplicantId));
    }

    public void Reject(string reason)
    {
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(reason), "Нужно указать причину отклонения.");

        MoveTo(ApplicationStatus.Rejected);
        DecisionComment = reason;
        Raise(new ApplicationRejected(Id, CohortId, ApplicantId, reason));
    }

    private void MoveTo(ApplicationStatus target)
    {
        var allowed = AllowedTransitions.TryGetValue(Status, out var targets) && targets.Contains(target);

        DomainException.ThrowIf(
            !allowed,
            $"Переход заявки из статуса «{Status}» в «{target}» недопустим.");

        Status = target;
        StatusChangedAtUtc = DateTime.UtcNow;
    }
}
