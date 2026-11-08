using SignalLab.Application.Organizations;
using SignalLab.Application.Users;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Tests.Organizations;

public sealed class GetOrganizationMembersTests
{
    [Fact]
    public async Task ExecuteAsync_WithInaccessibleOrganization_DoesNotFetchUsers()
    {
        var owner = CreateUser("owner", "owner@example.com");
        var removedUser = CreateUser("removed", "removed@example.com");

        var organization = CreateOrganization(owner.Id);

        var memberRole = organization.Roles.Single(
            role => role.Name == "Member");

        organization.AddMember(
            removedUser.Id,
            memberRole.Id,
            DateTimeOffset.UtcNow);

        organization.RemoveMember(
            removedUser.Id,
            DateTimeOffset.UtcNow);

        var scenarios = new[]
        {
            (
                Organization: (Organization?)organization,
                CurrentUserId: UserId.New()
            ),
            (
                Organization: (Organization?)organization,
                CurrentUserId: removedUser.Id
            ),
            (
                Organization: (Organization?)null,
                CurrentUserId: owner.Id
            )
        };

        foreach (var scenario in scenarios)
        {
            var userRepository = new FakeUserRepository(
                owner,
                removedUser);

            var useCase = CreateUseCase(
                scenario.Organization,
                userRepository);

            var result = await useCase.ExecuteAsync(
                organization.Id,
                scenario.CurrentUserId,
                CancellationToken.None);

            Assert.Null(result);
            Assert.Equal(0, userRepository.GetByIdsCallCount);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleMembers_FetchesUsersOnceAndSortsResults()
    {
        var owner = CreateUser("owner", "zeta@example.com");
        var memberB = CreateUser("member-b", "bravo@example.com");
        var memberA = CreateUser("member-a", "alpha@example.com");
        var removed = CreateUser("removed", "aardvark@example.com");

        var organization = CreateOrganization(owner.Id);

        var memberRole = organization.Roles.Single(
            role => role.Name == "Member");

        organization.AddMember(
            memberB.Id,
            memberRole.Id,
            DateTimeOffset.UtcNow);

        organization.AddMember(
            removed.Id,
            memberRole.Id,
            DateTimeOffset.UtcNow);

        organization.AddMember(
            memberA.Id,
            memberRole.Id,
            DateTimeOffset.UtcNow);

        organization.RemoveMember(
            removed.Id,
            DateTimeOffset.UtcNow);

        var userRepository = new FakeUserRepository(
            owner,
            memberB,
            memberA,
            removed);

        var useCase = CreateUseCase(
            organization,
            userRepository);

        var result = await useCase.ExecuteAsync(
            organization.Id,
            owner.Id,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(4, result.Count);

        Assert.Equal(
            new[]
            {
                "alpha@example.com",
                "bravo@example.com",
                "zeta@example.com",
                "aardvark@example.com"
            },
            result.Select(member => member.Email));

        Assert.Equal(
            new[]
            {
                OrganizationMemberStatus.Active,
                OrganizationMemberStatus.Active,
                OrganizationMemberStatus.Active,
                OrganizationMemberStatus.Removed
            },
            result.Select(member => member.Status));

        Assert.Equal(1, userRepository.GetByIdsCallCount);

        Assert.NotNull(userRepository.LastRequestedUserIds);
        Assert.Equal(4, userRepository.LastRequestedUserIds.Count);

        Assert.Equal(
            4,
            userRepository.LastRequestedUserIds.Distinct().Count());

        Assert.Contains(owner.Id, userRepository.LastRequestedUserIds);
        Assert.Contains(memberA.Id, userRepository.LastRequestedUserIds);
        Assert.Contains(memberB.Id, userRepository.LastRequestedUserIds);
        Assert.Contains(removed.Id, userRepository.LastRequestedUserIds);
    }

    private static User CreateUser(
        string externalIdentityId,
        string email)
    {
        return new User(
            externalIdentityId,
            email,
            DateTimeOffset.UtcNow);
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

    private static GetOrganizationMembers CreateUseCase(
        Organization? organization,
        FakeUserRepository userRepository)
    {
        var organizationRepository = new FakeOrganizationRepository(
            organization);

        var accessService = new OrganizationAccessService(
            organizationRepository);

        return new GetOrganizationMembers(
            accessService,
            userRepository);
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
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly IReadOnlyList<User> users;

        public int GetByIdsCallCount { get; private set; }

        public IReadOnlyCollection<UserId>? LastRequestedUserIds
        {
            get;
            private set;
        }

        public FakeUserRepository(params User[] users)
        {
            this.users = users;
        }

        public Task<IReadOnlyList<User>> GetByIdsAsync(
            IReadOnlyCollection<UserId> userIds,
            CancellationToken cancellationToken)
        {
            GetByIdsCallCount++;
            LastRequestedUserIds = userIds.ToArray();

            var result = users
                .Where(user => userIds.Contains(user.Id))
                .ToList();

            return Task.FromResult<IReadOnlyList<User>>(result);
        }

        public Task<User?> GetByIdAsync(
            UserId userId,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<User?> GetByExternalIdentityIdAsync(
            string externalIdentityId,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public void Add(User user)
        {
            throw new NotSupportedException();
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}