using SignalLab.Application.Errors;

namespace SignalLab.Application.Organizations;

public sealed class OrganizationInvitationAlreadyPendingException
    : ConflictException
{
    public OrganizationInvitationAlreadyPendingException()
        : base(
            OrganizationInvitationErrorCodes.AlreadyPending,
            "A pending invitation already exists for this user.")
    {
    }

    public OrganizationInvitationAlreadyPendingException(
        Exception innerException)
        : base(
            OrganizationInvitationErrorCodes.AlreadyPending,
            "A pending invitation already exists for this user.",
            innerException)
    {
    }
}