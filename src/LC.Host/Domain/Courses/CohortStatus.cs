namespace LC.Host.Domain.Courses;

public enum CohortStatus
{
    /// <summary>Черновик — запись ещё не открывалась. Единственный статус, в котором поток можно удалить.</summary>
    Draft = 1,

    /// <summary>Приём заявок открыт.</summary>
    EnrollmentOpen = 2,

    /// <summary>Приём заявок закрыт (досрочно вручную или по заполнению мест), обучение ещё не началось.</summary>
    EnrollmentClosed = 3,

    /// <summary>Обучение идёт.</summary>
    InProgress = 4,

    /// <summary>Поток завершён.</summary>
    Completed = 5,

    /// <summary>Поток отменён.</summary>
    Cancelled = 6,

    /// <summary>Архив. Терминальный статус вместо удаления.</summary>
    Archived = 7,
}

public enum EnrollmentClosureReason
{
    /// <summary>Приём закрыт вручную досрочно.</summary>
    ClosedManually = 1,

    /// <summary>Заполнены все места.</summary>
    CapacityReached = 2,
}
