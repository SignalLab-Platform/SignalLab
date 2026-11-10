using Microsoft.EntityFrameworkCore;
using SignalLab.Application.Organizations;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Infrastructure.Persistence.Repositories;

public sealed class OrganizationInvitationRepository : IOrganizationInvitationRepository
{
    private readonly SignalLabDbContext dbContext;

    public OrganizationInvitationRepository(SignalLabDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<OrganizationInvitation?> GetByIdAsync(
        OrganizationInvitationId invitationId,
        CancellationToken cancellationToken)
    {
        return dbContext.OrganizationInvitations
            .SingleOrDefaultAsync(
                invitation => invitation.Id == invitationId,
                cancellationToken);
    }

    public Task<OrganizationInvitation?> GetPendingAsync(
        OrganizationId organizationId,
        UserId invitedUserId,
        CancellationToken cancellationToken)
    {
        return dbContext.OrganizationInvitations
            .SingleOrDefaultAsync(
                invitation =>
                    invitation.OrganizationId == organizationId &&
                    invitation.InvitedUserId == invitedUserId &&
                    invitation.Status == OrganizationInvitationStatus.Pending,
                cancellationToken);
    }

    public async Task<IReadOnlyList<OrganizationInvitation>> GetByOrganizationIdAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken)
    {
        return await dbContext.OrganizationInvitations
            .AsNoTracking()
            .Where(invitation => invitation.OrganizationId == organizationId)
            .OrderByDescending(invitation => invitation.CreatedAt)
            .ThenBy(invitation => invitation.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OrganizationInvitation>> GetByInvitedUserIdAsync(
        UserId invitedUserId,
        CancellationToken cancellationToken)
    {
        return await dbContext.OrganizationInvitations
            .AsNoTracking()
            .Where(invitation => invitation.InvitedUserId == invitedUserId)
            .OrderByDescending(invitation => invitation.CreatedAt)
            .ThenBy(invitation => invitation.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<OrganizationInvitation?> GetByIdForUpdateAsync(
        OrganizationInvitationId invitationId,
        CancellationToken cancellationToken)
    {
        var invitations = await dbContext.OrganizationInvitations
            .FromSqlInterpolated(
                $"SELECT * FROM organization_invitations WHERE \"Id\" = {invitationId.Value} FOR UPDATE")
            .ToListAsync(cancellationToken);

        return invitations.SingleOrDefault();
    }

    public void Add(OrganizationInvitation invitation)
    {
        dbContext.OrganizationInvitations.Add(invitation);
    }
}