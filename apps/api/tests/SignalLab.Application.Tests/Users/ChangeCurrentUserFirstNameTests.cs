using SignalLab.Application.Users;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Tests.Users;

public sealed class ChangeCurrentUserFirstNameTests
{
    [Fact]
    public async Task ExecuteAsync_WhenUserExists_ChangesFirstName()
    {
        var user = CreateUser();
        var repository = new FakeUserRepository(user);
        var useCase = new ChangeCurrentUserFirstName(repository);

        var result = await useCase.ExecuteAsync(
            user.Id,
            "Jimmy",
            CancellationToken.None);

        Assert.Same(user, result);
        Assert.Equal("Jimmy", user.FirstName);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_UsesDomainFirstNameNormalization()
    {
        var user = CreateUser();
        var repository = new FakeUserRepository(user);
        var useCase = new ChangeCurrentUserFirstName(repository);

        await useCase.ExecuteAsync(
            user.Id,
            "  Jimmy  ",
            CancellationToken.None);

        Assert.Equal("Jimmy", user.FirstName);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserDoesNotExist_Fails()
    {
        var userId = UserId.New();
        var repository = new FakeUserRepository(null);
        var useCase = new ChangeCurrentUserFirstName(repository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => useCase.ExecuteAsync(
                userId,
                "Jimmy",
                CancellationToken.None));

        Assert.Equal(
            $"SignalLab user '{userId.Value}' could not be found.",
            exception.Message);

        Assert.Equal(0, repository.SaveChangesCallCount);
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

        public int SaveChangesCallCount { get; private set; }

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
            return Task.FromResult(
                user?.ExternalIdentityId == externalIdentityId
                    ? user
                    : null);
        }

        public Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public void Add(User user)
        {
            throw new InvalidOperationException(
                "This test does not support adding users.");
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCallCount++;

            return Task.CompletedTask;
        }
    }
}