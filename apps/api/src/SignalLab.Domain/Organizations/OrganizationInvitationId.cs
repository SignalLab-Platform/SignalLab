namespace SignalLab.Domain.Organizations;

public readonly record struct OrganizationInvitationId(Guid Value)
{
    public static OrganizationInvitationId New()
    {
        return new OrganizationInvitationId(Guid.NewGuid());
    }
}