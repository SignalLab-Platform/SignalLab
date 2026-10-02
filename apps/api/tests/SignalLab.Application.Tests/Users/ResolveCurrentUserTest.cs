using SignalLab.Application.Identity;
using SignalLab.Application.Users;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Tests.Users;

public sealed class ResolveCurrentUserTests
{
    [Fact]
    public async Task ExecuteAsync_WhenUserExists_ReturnsUser()
    {
        var existingUser = new User(
            "external-user-123",
            "user@example.com",
            DateTimeOffset.UtcNow);

        var repository = new FakeUserRepository(existingUser);
        var identityProvider = new FakeExternalIdentityProvider(null);
        var useCase = new ResolveCurrentUser(repository, identityProvider);

        var result = await useCase.ExecuteAsync(
            existingUser.ExternalIdentityId,
            CancellationToken.None);

        Assert.Same(existingUser, result);
        Assert.Equal(0, identityProvider.GetByIdCallCount);
        Assert.Null(repository.AddedUser);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserDoesNotExist_CreatesUserFromExternalIdentity()
    {
        var externalIdentity = new ExternalIdentity(
            "external-user-123",
            "user@example.com");

        var repository = new FakeUserRepository(null);
        var identityProvider = new FakeExternalIdentityProvider(externalIdentity);

        var useCase = new ResolveCurrentUser(
            repository,
            identityProvider);

        var result = await useCase.ExecuteAsync(
            externalIdentity.Id,
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(externalIdentity.Id, result.ExternalIdentityId);
        Assert.Equal(externalIdentity.Email, result.Email);
        Assert.Equal(1, identityProvider.GetByIdCallCount);
        Assert.Same(result, repository.AddedUser);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExternalIdentityDoesNotExist_Throws()
    {
        var repository = new FakeUserRepository(null);
        var identityProvider = new FakeExternalIdentityProvider(null);

        var useCase = new ResolveCurrentUser(
            repository,
            identityProvider);

        var exception = await Assert.ThrowsAsync<ExternalIdentityNotFoundException>(
            () => useCase.ExecuteAsync(
                "external-user-123",
                CancellationToken.None));

        Assert.Equal(
            "External identity 'external-user-123' could not be found.",
            exception.Message);

        Assert.Equal(1, identityProvider.GetByIdCallCount);
        Assert.Null(repository.AddedUser);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly User? user;

        public User? AddedUser { get; private set; }
        public int SaveChangesCallCount { get; private set; }

        public FakeUserRepository(User? user)
        {
            this.user = user;
        }

        public Task<User?> GetByExternalIdentityIdAsync(
            string externalIdentityId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                user?.ExternalIdentityId == externalIdentityId
                    ? user
                    : null);
        }

        public void Add(User user)
        {
            AddedUser = user;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCallCount++;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeExternalIdentityProvider : IExternalIdentityProvider
    {
        private readonly ExternalIdentity? identity;

        public int GetByIdCallCount { get; private set; }

        public FakeExternalIdentityProvider(ExternalIdentity? identity)
        {
            this.identity = identity;
        }

        public Task<ExternalIdentity?> GetByIdAsync(
            string externalIdentityId,
            CancellationToken cancellationToken)
        {
            GetByIdCallCount++;

            return Task.FromResult(identity);
        }
    }
}