using LC.Host.Domain.Common;

namespace LC.Host.Domain.Organizations.Events;

public sealed record OrganizationRegistered(OrganizationId OrganizationId, string Name) : DomainEvent;

public sealed record OrganizationSubmittedForVerification(OrganizationId OrganizationId) : DomainEvent;

public sealed record OrganizationVerified(OrganizationId OrganizationId) : DomainEvent;

public sealed record OrganizationVerificationRejected(OrganizationId OrganizationId, string Reason) : DomainEvent;
