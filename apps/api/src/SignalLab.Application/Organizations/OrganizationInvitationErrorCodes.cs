namespace SignalLab.Application.Organizations;

public static class OrganizationInvitationErrorCodes
{
    public const string InvitationNotFound = "organization_invitation_not_found";
    public const string OrganizationNotFound = "organization_not_found";
    public const string ManageMembersDenied = "organization_manage_members_denied";
    public const string InvalidRole = "organization_invitation_invalid_role";
    public const string InvalidDuration = "organization_invitation_invalid_duration";
    public const string AlreadyMember = "organization_already_member";
    public const string AlreadyPending = "organization_invitation_already_pending";
    public const string InvalidState = "organization_invitation_invalid_state";
    public const string InvalidRecipientId = "organization_invitation_invalid_recipient_id";
    public const string InvalidRoleId = "organization_invitation_invalid_role_id";
    public const string InvalidRecipientEmail = "organization_invitation_invalid_recipient_email";
    public const string OwnerInvitationDenied = "organization_invitation_owner_denied";
    public const string RecipientUnavailable = "organization_invitation_recipient_unavailable";
}