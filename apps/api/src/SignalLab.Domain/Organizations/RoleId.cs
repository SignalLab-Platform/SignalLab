namespace SignalLab.Domain.Organizations;

public readonly record struct RoleId(Guid Value)
{
    public static RoleId New()
    {
        return new RoleId(Guid.NewGuid());
    }
}