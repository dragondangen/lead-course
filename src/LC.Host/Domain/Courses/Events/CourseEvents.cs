using LC.Host.Domain.Common;
using LC.Host.Domain.Organizations;

namespace LC.Host.Domain.Courses.Events;

public sealed record CourseCreated(CourseId CourseId, OrganizationId OrganizationId) : DomainEvent;

public sealed record CoursePublished(CourseId CourseId) : DomainEvent;

public sealed record CourseArchived(CourseId CourseId) : DomainEvent;
