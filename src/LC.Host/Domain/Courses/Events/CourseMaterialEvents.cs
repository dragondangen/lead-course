using LC.Host.Domain.Common;

namespace LC.Host.Domain.Courses.Events;

public sealed record CourseMaterialAdded(CourseMaterialId CourseMaterialId, CourseId CourseId) : DomainEvent;
