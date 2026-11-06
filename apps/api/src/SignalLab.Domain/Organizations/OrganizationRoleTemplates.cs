namespace SignalLab.Domain.Organizations;

public static class OrganizationRoleTemplates
{
    public static IReadOnlyList<Role> Create(
        OrganizationId organizationId,
        DateTimeOffset createdAt)
    {
        var memberPermissions = new[]
        {
            Permission.ManageProjects,
            Permission.ManageCampaigns,
            Permission.ManageTemplates,
            Permission.ManageMeasures,
            Permission.ViewAnalytics,
            Permission.ExportData
        };

        var administratorPermissions = memberPermissions
            .Append(Permission.ManageOrganization)
            .Append(Permission.ManageMembers)
            .ToArray();

        var ownerPermissions = administratorPermissions
            .Append(Permission.ManageOwnership)
            .ToArray();

        return new List<Role>
        {
            new Role(
                organizationId,
                "Owner",
                "Organization ownership and administration.",
                true,
                ownerPermissions,
                createdAt),

            new Role(
                organizationId,
                "Administrator",
                "Organization and member administration.",
                false,
                administratorPermissions,
                createdAt),

            new Role(
                organizationId,
                "Member",
                "Organization research activities.",
                false,
                memberPermissions,
                createdAt)
        };
    }
}
