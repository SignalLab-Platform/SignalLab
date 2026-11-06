using SignalLab.Domain.Users;

namespace SignalLab.Domain.Organizations;

public sealed class Organization
{
    private readonly List<Role> roles = new();
    private readonly List<OrganizationMember> members = new();

    public OrganizationId Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Country { get; private set; } = string.Empty;

    public string? Website { get; private set; }

    public IReadOnlyList<Role> Roles => roles.AsReadOnly();

    public IReadOnlyCollection<OrganizationMember> Members => members.AsReadOnly();

    public Role OwnerRole => roles.Single(role => role.IsOwner);

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    private Organization()
    {
    }

    public Organization(
        UserId creatorId,
        string name,
        string country,
        string? website,
        DateTimeOffset createdAt)
    {
        if (creatorId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "CreatorId cannot be empty.",
                nameof(creatorId));
        }

        Id = new OrganizationId(Guid.NewGuid());
        Name = ValidateRequired(name, nameof(name));
        Country = ValidateCountry(country);
        Website = ValidateWebsite(website);
        CreatedAt = createdAt;
        UpdatedAt = createdAt;

        roles.AddRange(
            OrganizationRoleTemplates.Create(Id, createdAt));

        members.Add(
            new OrganizationMember(
                Id,
                creatorId,
                OwnerRole.Id,
                createdAt));
    }

    public OrganizationMember AddMember(
        UserId userId,
        RoleId roleId,
        DateTimeOffset joinedAt)
    {
        ValidateUserId(userId);
        GetRole(roleId);

        if (members.Any(member => member.UserId == userId))
        {
            throw new InvalidOperationException(
                "User already has a membership in this organization.");
        }

        var member = new OrganizationMember(
            Id,
            userId,
            roleId,
            joinedAt);

        members.Add(member);
        UpdatedAt = joinedAt;

        return member;
    }

    public void RemoveMember(
        UserId userId,
        DateTimeOffset removedAt)
    {
        var member = GetMember(userId);

        if (member.Status != OrganizationMemberStatus.Active)
        {
            throw new InvalidOperationException(
                "Only active members can be removed.");
        }

        EnsureOwnerContinuity(member, null);

        member.Remove(removedAt);
        UpdatedAt = removedAt;
    }

    public OrganizationMember ReactivateMember(
        UserId userId,
        RoleId roleId,
        DateTimeOffset reactivatedAt)
    {
        var member = GetMember(userId);
        GetRole(roleId);

        if (member.Status != OrganizationMemberStatus.Removed)
        {
            throw new InvalidOperationException(
                "Only removed members can be reactivated.");
        }

        member.Reactivate(roleId);
        UpdatedAt = reactivatedAt;

        return member;
    }

    public void ChangeMemberRole(
        UserId userId,
        RoleId roleId,
        DateTimeOffset changedAt)
    {
        var member = GetMember(userId);
        GetRole(roleId);

        if (member.Status != OrganizationMemberStatus.Active)
        {
            throw new InvalidOperationException(
                "Only active members can change roles.");
        }

        if (member.RoleId == roleId)
        {
            return;
        }

        EnsureOwnerContinuity(member, roleId);

        member.ChangeRole(roleId);
        UpdatedAt = changedAt;
    }

    private OrganizationMember GetMember(UserId userId)
    {
        ValidateUserId(userId);

        return members.SingleOrDefault(
            member => member.UserId == userId)
            ?? throw new InvalidOperationException(
                "User is not a member of this organization.");
    }

    private Role GetRole(RoleId roleId)
    {
        if (roleId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "RoleId cannot be empty.",
                nameof(roleId));
        }

        return roles.SingleOrDefault(
            role => role.Id == roleId)
            ?? throw new InvalidOperationException(
                "Role does not belong to this organization.");
    }

    private void EnsureOwnerContinuity(
        OrganizationMember member,
        RoleId? replacementRoleId)
    {
        if (member.RoleId != OwnerRole.Id)
        {
            return;
        }

        if (replacementRoleId == OwnerRole.Id)
        {
            return;
        }

        var activeOwnerCount = members.Count(
            candidate =>
                candidate.Status == OrganizationMemberStatus.Active &&
                candidate.RoleId == OwnerRole.Id);

        if (activeOwnerCount <= 1)
        {
            throw new InvalidOperationException(
                "The last active owner cannot be removed or demoted.");
        }
    }

    private static void ValidateUserId(UserId userId)
    {
        if (userId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "UserId cannot be empty.",
                nameof(userId));
        }
    }

    private static string ValidateRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Value cannot be empty.",
                parameterName);
        }

        return value.Trim();
    }

    private static string ValidateCountry(string country)
    {
        var normalizedCountry = ValidateRequired(country, nameof(country));

        if (!IsoCountryCodes.Contains(normalizedCountry))
        {
            throw new ArgumentException(
                "Country must be a valid ISO 3166-1 alpha-2 code.",
                nameof(country));
        }

        return normalizedCountry;
    }

    private static string? ValidateWebsite(string? website)
    {
        if (string.IsNullOrWhiteSpace(website))
        {
            return null;
        }

        var normalizedWebsite = website.Trim();

        if (!Uri.TryCreate(
            normalizedWebsite,
            UriKind.Absolute,
            out var uri))
        {
            throw new ArgumentException(
                "Website must be a valid absolute URL.",
                nameof(website));
        }

        if (uri.Scheme != Uri.UriSchemeHttp &&
            uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException(
                "Website must use HTTP or HTTPS.",
                nameof(website));
        }

        return normalizedWebsite;
    }
}