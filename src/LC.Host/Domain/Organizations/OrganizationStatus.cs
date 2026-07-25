namespace LC.Host.Domain.Organizations;

public enum OrganizationStatus
{
    /// <summary>Зарегистрирована, документы ещё не поданы.</summary>
    Registered = 1,

    /// <summary>Документы поданы, идёт проверка.</summary>
    OnVerification = 2,

    /// <summary>Проверка пройдена — организация может публиковать курсы.</summary>
    Verified = 3,

    /// <summary>Проверка не пройдена, можно подать документы повторно.</summary>
    VerificationRejected = 4,
}
