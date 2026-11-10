using SignalLab.Application.Errors;
using SignalLab.Domain.Organizations;

namespace SignalLab.Application.Organizations;

internal static class OrganizationInvitationTransitionValidator
{
    public static void EnsureModifiable(
        OrganizationInvitation invitation,
        DateTimeOffset now)
    {
        if (invitation.Status != OrganizationInvitationStatus.Pending)
        {
            throw new ConflictException(
                OrganizationInvitationErrorCodes.InvalidState,
                "Organization invitation is no longer pending.");
        }

        if (now < invitation.CreatedAt)
        {
            throw new InvalidOperationException(
                "An invitation cannot be modified before its creation.");
        }

        if (now >= invitation.ExpiresAt)
        {
            throw new ConflictException(
                OrganizationInvitationErrorCodes.InvalidState,
                "Organization invitation has expired.");
        }
    }
}