using SignalLab.Application.Organizations;
using SignalLab.Application.Users;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Tests.Organizations;

public sealed class CreateOrganizationTests
{
    [Fact]
    public async Task ExecuteAsync_WhenCreatorExists_CreatesOrganization()
    {
        var user = CreateUser();
        var userRepository = new FakeUserRepository(user);
        var organizationRepository = new FakeOrganizationRepository();

        var useCase = new CreateOrganization(
            organizationRepository,
            userRepository);

        var result = await useCase.ExecuteAsync(
            user.Id,
            "SignalLab Studio",
            "FR",
            "https://example.com",
            CancellationToken.None);

        Assert.Same(result, organizationRepository.AddedOrganization);

        Assert.Equal("SignalLab Studio", result.Name);
        Assert.Equal("FR", result.Country);
        Assert.Equal("https://example.com", result.Website);

        Assert.Equal(3, result.Roles.Count);
        Assert.Single(result.Members);

        var ownerMembership = Assert.Single(result.Members);

        Assert.Equal(user.Id, ownerMembership.UserId);
        Assert.Equal(result.OwnerRole.Id, ownerMembership.RoleId);
        Assert.Equal(OrganizationMemberStatus.Active, ownerMembership.Status);

        Assert.Equal(1, organizationRepository.AddCallCount);
        Assert.Equal(1, organizationRepository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_UsesDomainNormalization()
    {
        var user = CreateUser();
        var userRepository = new FakeUserRepository(user);
        var organizationRepository = new FakeOrganizationRepository();

        var useCase = new CreateOrganization(
            organizationRepository,
            userRepository);

        var result = await useCase.ExecuteAsync(
            user.Id,
            "  SignalLab Studio  ",
            "  FR  ",
            "  https://example.com  ",
            CancellationToken.None);

        Assert.Equal("SignalLab Studio", result.Name);
        Assert.Equal("FR", result.Country);
        Assert.Equal("https://example.com", result.Website);

        Assert.Equal(1, organizationRepository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCreatorDoesNotExist_Fails()
    {
        var userId = UserId.New();
        var userRepository = new FakeUserRepository(null);
        var organizationRepository = new FakeOrganizationRepository();

        var useCase = new CreateOrganization(
            organizationRepository,
            userRepository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                userId,
                "SignalLab Studio",
                "FR",
                null,
                CancellationToken.None));

        Assert.Equal(
            $"SignalLab user '{userId.Value}' could not be found.",
            exception.Message);

        Assert.Null(organizationRepository.AddedOrganization);
        Assert.Equal(0, organizationRepository.AddCallCount);
        Assert.Equal(0, organizationRepository.SaveChangesCallCount);
    }

    [Theory]
    [InlineData("", "FR")]
    [InlineData("   ", "FR")]
    [InlineData("SignalLab Studio", "")]
    [InlineData("SignalLab Studio", "   ")]
    public async Task ExecuteAsync_WithInvalidRequiredFields_Fails(
        string name,
        string country)
    {
        var user = CreateUser();
        var userRepository = new FakeUserRepository(user);
        var organizationRepository = new FakeOrganizationRepository();

        var useCase = new CreateOrganization(
            organizationRepository,
            userRepository);

        await Assert.ThrowsAnyAsync<ArgumentException>(
            () => useCase.ExecuteAsync(
                user.Id,
                name,
                country,
                null,
                CancellationToken.None));

        Assert.Null(organizationRepository.AddedOrganization);
        Assert.Equal(0, organizationRepository.AddCallCount);
        Assert.Equal(0, organizationRepository.SaveChangesCallCount);
    }

    [Theory]
    [InlineData("France")]
    [InlineData("FRA")]
    [InlineData("ZZ")]
    [InlineData("fr")]
    [InlineData("123")]
    public async Task ExecuteAsync_WithInvalidCountryCode_DoesNotPersistOrganization(
        string country)
    {
        var user = CreateUser();
        var userRepository = new FakeUserRepository(user);
        var organizationRepository = new FakeOrganizationRepository();

        var useCase = new CreateOrganization(
            organizationRepository,
            userRepository);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.ExecuteAsync(
                user.Id,
                "SignalLab Studio",
                country,
                null,
                CancellationToken.None));

        Assert.Equal("country", exception.ParamName);

        Assert.Null(organizationRepository.AddedOrganization);
        Assert.Equal(0, organizationRepository.AddCallCount);
        Assert.Equal(0, organizationRepository.SaveChangesCallCount);
    }

    private static User CreateUser()
    {
        return new User(
            "external-user-123",
            "user@example.com",
            DateTimeOffset.UtcNow);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly User? user;

        public FakeUserRepository(User? user)
        {
            this.user = user;
        }

        public Task<User?> GetByIdAsync(
            UserId userId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                user?.Id == userId
                    ? user
                    : null);
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

    private sealed class FakeOrganizationRepository : IOrganizationRepository
    {
        public Organization? AddedOrganization { get; private set; }

        public int AddCallCount { get; private set; }

        public int SaveChangesCallCount { get; private set; }

        public Task<Organization?> GetByIdAsync(
            OrganizationId organizationId,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public void Add(Organization organization)
        {
            AddedOrganization = organization;
            AddCallCount++;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCallCount++;

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Organization>> GetAccessibleByUserIdAsync(
            UserId userId,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}