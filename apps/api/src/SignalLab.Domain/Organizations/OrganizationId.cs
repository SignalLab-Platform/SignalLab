namespace SignalLab.Domain.Organizations;

public readonly record struct OrganizationId(Guid Value)
{
    public static OrganizationId New()
    {
        return new OrganizationId(Guid.NewGuid());
    }
}