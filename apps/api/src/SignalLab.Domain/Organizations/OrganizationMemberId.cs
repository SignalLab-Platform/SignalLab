namespace SignalLab.Domain.Organizations;

public readonly record struct OrganizationMemberId(Guid Value)
{
    public static OrganizationMemberId New()
    {
        return new OrganizationMemberId(Guid.NewGuid());
    }
}