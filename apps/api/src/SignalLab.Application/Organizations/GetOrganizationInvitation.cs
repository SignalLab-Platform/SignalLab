using SignalLab.Application.Users;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;
using SignalLab.Application.Errors;

namespace SignalLab.Application.Organizations;

public sealed class GetOrganizationInvitation
{
    private readonly IOrganizationInvitationRepository invitationRepository;
    private readonly IOrganizationRepository organizationRepository;
    private readonly OrganizationAccessService organizationAccessService;
    private readonly IUserRepository userRepository;

    public GetOrganizationInvitation(
        IOrganizationInvitationRepository invitationRepository,
        IOrganizationRepository organizationRepository,
        OrganizationAccessService organizationAccessService,
        IUserRepository userRepository)
    {
        this.invitationRepository = invitationRepository;
        this.organizationRepository = organizationRepository;
        this.organizationAccessService = organizationAccessService;
        this.userRepository = userRepository;
    }

    public async Task<OrganizationInvitationDetails?> ExecuteAsync(
        OrganizationInvitationId invitationId,
        UserId currentUserId,
        CancellationToken cancellationToken)
    {
        var invitation = await invitationRepository.GetByIdAsync(
            invitationId,
            cancellationToken);

        if (invitation is null)
        {
            return null;
        }

        Organization? organization;

        if (invitation.InvitedUserId == currentUserId)
        {
            var organizations = await organizationRepository.GetByIdsAsync(
                [invitation.OrganizationId],
                cancellationToken);

            organization = organizations.SingleOrDefault();
        }
        else
        {
            organization = await organizationAccessService.GetAccessibleAsync(
                invitation.OrganizationId,
                currentUserId,
                cancellationToken);
        }

        if (organization is null)
        {
            return null;
        }

        var invitedUser = await userRepository.GetByIdAsync(
            invitation.InvitedUserId,
            cancellationToken);

        if (invitedUser is null)
        {
            throw new ResourceNotFoundException(
                UserErrorCodes.UserNotFound,
                "Invited user could not be found.");
        }

        return OrganizationInvitationDetails.From(
            invitation,
            organization,
            invitedUser,
            DateTimeOffset.UtcNow);
    }
}