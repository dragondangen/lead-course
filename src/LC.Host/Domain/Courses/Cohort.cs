using LC.Host.Domain.Common;
using LC.Host.Domain.Courses.Events;

namespace LC.Host.Domain.Courses;

/// <summary>
/// Поток курса (например, «июнь — август») с ограниченным числом мест.
/// Заявки живут в поддомене Enrollment; здесь — только счётчик занятых мест,
/// который меняется при одобрении заявки под оптимистичной блокировкой.
/// Хранить коллекцию из сотен тысяч заявок внутри агрегата нельзя.
/// </summary>
public sealed class Cohort : AggregateRoot<CohortId>
{
    private Cohort()
    {
    }

    private Cohort(CohortId id, CourseId courseId, string name, DateRange period, int totalSeats)
        : base(id)
    {
        CourseId = courseId;
        Name = name;
        Period = period;
        TotalSeats = totalSeats;
        Status = CohortStatus.Draft;
    }

    public CourseId CourseId { get; private set; }

    public string Name { get; private set; } = null!;

    public DateRange Period { get; private set; } = null!;

    public int TotalSeats { get; private set; }

    /// <summary>Места, занятые одобренными заявками.</summary>
    public int ReservedSeats { get; private set; }

    public CohortStatus Status { get; private set; }

    public string? CancellationReason { get; private set; }

    public int AvailableSeats => TotalSeats - ReservedSeats;

    /// <summary>
    /// Удалить можно только поток, на который запись ещё не открывалась.
    /// Остальные — только архивировать.
    /// </summary>
    public bool CanBeDeleted => Status == CohortStatus.Draft;

    /// <param name="courseIsPublished">
    /// Статус курса (<see cref="Course.Status"/> == Published); загружается application-слоем.
    /// </param>
    public static Cohort Create(CourseId courseId, bool courseIsPublished, string name, DateRange period, int totalSeats)
    {
        DomainException.ThrowIf(!courseIsPublished, "Потоки можно создавать только у опубликованного курса.");
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(name), "У потока должно быть название.");
        DomainException.ThrowIf(totalSeats <= 0, "Количество мест на потоке должно быть больше нуля.");

        return new Cohort(CohortId.New(), courseId, name.Trim(), period, totalSeats);
    }

    public void OpenEnrollment()
    {
        DomainException.ThrowIf(Status != CohortStatus.Draft, "Открыть приём заявок можно только из черновика.");

        Status = CohortStatus.EnrollmentOpen;
        Raise(new CohortEnrollmentOpened(Id, CourseId));
    }

    /// <summary>Досрочное закрытие приёма, не дожидаясь заполнения мест.</summary>
    public void CloseEnrollment() => CloseEnrollment(EnrollmentClosureReason.ClosedManually);

    /// <summary>
    /// Занять место одобренной заявкой. Вызывается при одобрении заявки в поддомене Enrollment;
    /// конкурентные одобрения разруливаются оптимистичной блокировкой на агрегате.
    /// Разрешено и после закрытия приёма: заявки, поданные до закрытия, продолжают обрабатываться.
    /// </summary>
    public void ReserveSeat()
    {
        DomainException.ThrowIf(
            Status is not (CohortStatus.EnrollmentOpen or CohortStatus.EnrollmentClosed),
            "Занять место можно только до начала обучения.");
        DomainException.ThrowIf(AvailableSeats == 0, "На потоке не осталось свободных мест.");

        ReservedSeats++;
        Raise(new CohortSeatReserved(Id, ReservedSeats, TotalSeats));

        if (AvailableSeats == 0 && Status == CohortStatus.EnrollmentOpen)
        {
            CloseEnrollment(EnrollmentClosureReason.CapacityReached);
        }
    }

    /// <summary>Освободить место (одобренная заявка отозвана до старта обучения).</summary>
    public void ReleaseSeat()
    {
        DomainException.ThrowIf(ReservedSeats == 0, "На потоке нет занятых мест.");

        ReservedSeats--;
    }

    public void Start()
    {
        DomainException.ThrowIf(
            Status is not (CohortStatus.EnrollmentOpen or CohortStatus.EnrollmentClosed),
            "Начать обучение можно только на потоке с открытым или закрытым приёмом заявок.");

        if (Status == CohortStatus.EnrollmentOpen)
        {
            CloseEnrollment(EnrollmentClosureReason.ClosedManually);
        }

        Status = CohortStatus.InProgress;
        Raise(new CohortStarted(Id, CourseId));
    }

    public void Complete()
    {
        DomainException.ThrowIf(Status != CohortStatus.InProgress, "Завершить можно только идущий поток.");

        Status = CohortStatus.Completed;
        Raise(new CohortCompleted(Id, CourseId));
    }

    public void Cancel(string reason)
    {
        DomainException.ThrowIf(
            Status is CohortStatus.Completed or CohortStatus.Cancelled or CohortStatus.Archived,
            "Завершённый, отменённый или архивный поток отменить нельзя.");
        DomainException.ThrowIf(string.IsNullOrWhiteSpace(reason), "Нужно указать причину отмены потока.");

        Status = CohortStatus.Cancelled;
        CancellationReason = reason;
        Raise(new CohortCancelled(Id, CourseId, reason));
    }

    public void Archive()
    {
        DomainException.ThrowIf(
            Status is not (CohortStatus.Completed or CohortStatus.Cancelled),
            "Архивировать можно только завершённый или отменённый поток.");

        Status = CohortStatus.Archived;
        Raise(new CohortArchived(Id));
    }

    private void CloseEnrollment(EnrollmentClosureReason reason)
    {
        DomainException.ThrowIf(
            Status != CohortStatus.EnrollmentOpen,
            "Закрыть приём заявок можно только когда он открыт.");

        Status = CohortStatus.EnrollmentClosed;
        Raise(new CohortEnrollmentClosed(Id, CourseId, reason));
    }
}
