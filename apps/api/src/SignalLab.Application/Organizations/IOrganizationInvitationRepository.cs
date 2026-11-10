using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Organizations;

public interface IOrganizationInvitationRepository
{
    Task<OrganizationInvitation?> GetByIdAsync(
        OrganizationInvitationId invitationId,
        CancellationToken cancellationToken);

    Task<OrganizationInvitation?> GetPendingAsync(
        OrganizationId organizationId,
        UserId invitedUserId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<OrganizationInvitation>> GetByOrganizationIdAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<OrganizationInvitation>> GetByInvitedUserIdAsync(
        UserId invitedUserId,
        CancellationToken cancellationToken);
        
    Task<OrganizationInvitation?> GetByIdForUpdateAsync(
        OrganizationInvitationId invitationId,
        CancellationToken cancellationToken);

    void Add(OrganizationInvitation invitation);
}