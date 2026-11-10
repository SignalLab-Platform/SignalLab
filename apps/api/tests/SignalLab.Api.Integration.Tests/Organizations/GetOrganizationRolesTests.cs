using SignalLab.Application.Organizations;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Tests.Organizations;

public sealed class GetOrganizationRolesTests
{
    [Fact]
    public async Task ExecuteAsync_WithInaccessibleOrganization_ReturnsNull()
    {
        var ownerId = UserId.New();
        var removedUserId = UserId.New();
        var organization = CreateOrganization(ownerId);

        var memberRole = organization.Roles.Single(
            role => role.Name == "Member");

        organization.AddMember(
            removedUserId,
            memberRole.Id,
            DateTimeOffset.UtcNow);

        organization.RemoveMember(
            removedUserId,
            DateTimeOffset.UtcNow);

        var scenarios = new[]
        {
            (
                Organization: (Organization?)organization,
                CurrentUserId: UserId.New()
            ),
            (
                Organization: (Organization?)organization,
                CurrentUserId: removedUserId
            ),
            (
                Organization: (Organization?)null,
                CurrentUserId: ownerId
            )
        };

        foreach (var scenario in scenarios)
        {
            var useCase = CreateUseCase(scenario.Organization);

            var result = await useCase.ExecuteAsync(
                organization.Id,
                scenario.CurrentUserId,
                CancellationToken.None);

            Assert.Null(result);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithAccessibleOrganization_ReturnsSortedRoles()
    {
        var ownerId = UserId.New();
        var organization = CreateOrganization(ownerId);

        var useCase = CreateUseCase(organization);

        var result = await useCase.ExecuteAsync(
            organization.Id,
            ownerId,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);

        Assert.Equal(
            new[]
            {
                "Owner",
                "Administrator",
                "Member"
            },
            result.Select(role => role.Name));

        Assert.True(result[0].IsOwner);

        Assert.All(
            result.Skip(1),
            role => Assert.False(role.IsOwner));

        Assert.All(
            result,
            role => Assert.NotEmpty(role.Permissions));
    }

    private static Organization CreateOrganization(UserId ownerId)
    {
        return new Organization(
            ownerId,
            "SignalLab Studio",
            "FR",
            null,
            DateTimeOffset.UtcNow);
    }

    private static GetOrganizationRoles CreateUseCase(
        Organization? organization)
    {
        var repository = new FakeOrganizationRepository(organization);

        var accessService = new OrganizationAccessService(repository);

        return new GetOrganizationRoles(accessService);
    }

    private sealed class FakeOrganizationRepository : IOrganizationRepository
    {
        private readonly Organization? organization;

        public FakeOrganizationRepository(Organization? organization)
        {
            this.organization = organization;
        }

        public Task<Organization?> GetByIdAsync(
            OrganizationId organizationId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                organization?.Id == organizationId
                    ? organization
                    : null);
        }

        public Task<IReadOnlyList<Organization>> GetAccessibleByUserIdAsync(
            UserId userId,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public void Add(Organization organization)
        {
            throw new NotSupportedException();
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<Organization>> GetByIdsAsync(
            IReadOnlyCollection<OrganizationId> organizationIds,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}