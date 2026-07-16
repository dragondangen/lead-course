using LC.Host.Domain.Common;
using LC.Host.Domain.Courses;

namespace LC.Host.Domain.Enrollment.Events;

public sealed record ApplicationSubmitted(ApplicationId ApplicationId, CohortId CohortId, UserId ApplicantId) : DomainEvent;

public sealed record ApplicationTakenInReview(ApplicationId ApplicationId, UserId ApplicantId) : DomainEvent;

public sealed record ApplicationAgreed(ApplicationId ApplicationId, UserId ApplicantId) : DomainEvent;

public sealed record ApplicationNotAgreed(ApplicationId ApplicationId, UserId ApplicantId, string Reason) : DomainEvent;

public sealed record ApplicationApproved(ApplicationId ApplicationId, CohortId CohortId, UserId ApplicantId) : DomainEvent;

public sealed record ApplicationRejected(ApplicationId ApplicationId, CohortId CohortId, UserId ApplicantId, string Reason) : DomainEvent;
