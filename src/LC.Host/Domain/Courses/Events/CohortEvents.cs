using LC.Host.Domain.Common;

namespace LC.Host.Domain.Courses.Events;

public sealed record CohortEnrollmentOpened(CohortId CohortId, CourseId CourseId) : DomainEvent;

public sealed record CohortEnrollmentClosed(
    CohortId CohortId,
    CourseId CourseId,
    EnrollmentClosureReason Reason) : DomainEvent;

public sealed record CohortSeatReserved(CohortId CohortId, int ReservedSeats, int TotalSeats) : DomainEvent;

public sealed record CohortStarted(CohortId CohortId, CourseId CourseId) : DomainEvent;

public sealed record CohortCompleted(CohortId CohortId, CourseId CourseId) : DomainEvent;

public sealed record CohortCancelled(CohortId CohortId, CourseId CourseId, string Reason) : DomainEvent;

public sealed record CohortArchived(CohortId CohortId) : DomainEvent;
