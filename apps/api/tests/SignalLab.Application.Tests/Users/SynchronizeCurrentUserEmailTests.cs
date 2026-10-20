using SignalLab.Application.Identity;
using SignalLab.Application.Users;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Tests.Users;

public sealed class SynchronizeCurrentUserEmailTests
{
    [Fact]
    public async Task ExecuteAsync_WhenExternalEmailChanged_SynchronizesEmail()
    {
        var user = CreateUser();

        var repository = new FakeUserRepository(user);

        var externalIdentityProvider =
            new FakeExternalIdentityProvider(
                new ExternalIdentity(
                    user.ExternalIdentityId,
                    " New@Example.com "));

        var useCase = new SynchronizeCurrentUserEmail(
            repository,
            externalIdentityProvider);

        var result = await useCase.ExecuteAsync(
            user.Id,
            CancellationToken.None);

        Assert.Same(user, result);
        Assert.Equal("new@example.com", user.Email);
        Assert.Equal(1, repository.SaveChangesCallCount);

        Assert.Equal(
            user.ExternalIdentityId,
            externalIdentityProvider.RequestedExternalIdentityId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserDoesNotExist_Fails()
    {
        var userId = UserId.New();

        var repository = new FakeUserRepository();

        var externalIdentityProvider =
            new FakeExternalIdentityProvider(null);

        var useCase = new SynchronizeCurrentUserEmail(
            repository,
            externalIdentityProvider);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                userId,
                CancellationToken.None));

        Assert.Equal(
            $"SignalLab user '{userId.Value}' could not be found.",
            exception.Message);

        Assert.Equal(
            0,
            externalIdentityProvider.GetByIdCallCount);

        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenExternalIdentityDoesNotExist_Fails()
    {
        var user = CreateUser();

        var repository = new FakeUserRepository(user);

        var externalIdentityProvider =
            new FakeExternalIdentityProvider(null);

        var useCase = new SynchronizeCurrentUserEmail(
            repository,
            externalIdentityProvider);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                user.Id,
                CancellationToken.None));

        Assert.Equal(
            $"External identity '{user.ExternalIdentityId}' could not be found.",
            exception.Message);

        Assert.Equal(
            1,
            externalIdentityProvider.GetByIdCallCount);

        Assert.Equal("user@example.com", user.Email);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmailBelongsToAnotherUser_Fails()
    {
        var user = CreateUser();

        var otherUser = new User(
            "external-user-456",
            "other@example.com",
            DateTimeOffset.UtcNow);

        var repository = new FakeUserRepository(
            user,
            otherUser);

        var externalIdentityProvider =
            new FakeExternalIdentityProvider(
                new ExternalIdentity(
                    user.ExternalIdentityId,
                    otherUser.Email));

        var useCase = new SynchronizeCurrentUserEmail(
            repository,
            externalIdentityProvider);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                user.Id,
                CancellationToken.None));

        Assert.Equal(
            "Email is already associated with another SignalLab user.",
            exception.Message);

        Assert.Equal("user@example.com", user.Email);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenEmailAlreadyMatches_Succeeds()
    {
        var user = CreateUser();

        var repository = new FakeUserRepository(user);

        var externalIdentityProvider =
            new FakeExternalIdentityProvider(
                new ExternalIdentity(
                    user.ExternalIdentityId,
                    user.Email));

        var useCase = new SynchronizeCurrentUserEmail(
            repository,
            externalIdentityProvider);

        var result = await useCase.ExecuteAsync(
            user.Id,
            CancellationToken.None);

        Assert.Same(user, result);
        Assert.Equal("user@example.com", user.Email);
        Assert.Equal(1, repository.SaveChangesCallCount);
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
        private readonly List<User> users;

        public FakeUserRepository(params User?[] users)
        {
            this.users = users
                .Where(user => user is not null)
                .Select(user => user!)
                .ToList();
        }

        public int SaveChangesCallCount { get; private set; }

        public Task<User?> GetByIdAsync(
            UserId userId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                users.SingleOrDefault(
                    user => user.Id == userId));
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
            var normalizedEmail = email
                .Trim()
                .ToLowerInvariant();

            return Task.FromResult(
                users.SingleOrDefault(
                    user => user.Email == normalizedEmail));
        }

        public void Add(User user)
        {
            throw new NotSupportedException();
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken)
        {
            SaveChangesCallCount++;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeExternalIdentityProvider
        : IExternalIdentityProvider
    {
        private readonly ExternalIdentity? externalIdentity;

        public FakeExternalIdentityProvider(
            ExternalIdentity? externalIdentity)
        {
            this.externalIdentity = externalIdentity;
        }

        public int GetByIdCallCount { get; private set; }

        public string? RequestedExternalIdentityId { get; private set; }

        public Task<ExternalIdentity?> GetByIdAsync(
            string externalIdentityId,
            CancellationToken cancellationToken)
        {
            GetByIdCallCount++;
            RequestedExternalIdentityId = externalIdentityId;

            if (externalIdentity?.Id != externalIdentityId)
            {
                return Task.FromResult<ExternalIdentity?>(null);
            }

            return Task.FromResult<ExternalIdentity?>(
                externalIdentity);
        }
    }
}