namespace SignalLab.Domain.Organizations;

public sealed class Role
{
    private readonly List<Permission> permissions = new();

    public RoleId Id { get; private set; }

    public OrganizationId OrganizationId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsOwner { get; private set; }

    public IReadOnlyCollection<Permission> Permissions => permissions.AsReadOnly();

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    private Role()
    {
    }

    public Role(
        OrganizationId organizationId,
        string name,
        string? description,
        bool isOwner,
        IEnumerable<Permission> permissions,
        DateTimeOffset createdAt)
    {
        if (organizationId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "OrganizationId cannot be empty.",
                nameof(organizationId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Role name cannot be empty.",
                nameof(name));
        }

        ArgumentNullException.ThrowIfNull(permissions);

        List<Permission> distinctPermissions = permissions.Distinct().ToList();

        if (distinctPermissions.Count == 0)
        {
            throw new ArgumentException(
                "A role must contain at least one permission.",
                nameof(permissions));
        }

        if (distinctPermissions.Any(permission => !Enum.IsDefined(permission)))
        {
            throw new ArgumentException(
                "A role contains an unknown permission.",
                nameof(permissions));
        }

        Id = RoleId.New();
        OrganizationId = organizationId;
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        IsOwner = isOwner;

        this.permissions.AddRange(distinctPermissions);

        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public bool HasPermission(Permission permission)
    {
        return permissions.Contains(permission);
    }
}