using SignalLab.Application.Users;
using SignalLab.Domain.Users;

namespace SignalLab.Application.Tests.Users;

public sealed class GetCurrentUserProfileTests
{
    [Fact]
    public async Task ExecuteAsync_WithExistingUser_ReturnsUser()
    {
        var user = new User(
            "user_test_123",
            "user@example.com",
            DateTimeOffset.UtcNow);

        user.ChangeFirstName("Jimmy");
        user.ChangeLastName("Fromonot");

        var repository = new FakeUserRepository(user);

        var useCase = new GetCurrentUserProfile(repository);

        var result = await useCase.ExecuteAsync(
            user.Id,
            CancellationToken.None);

        Assert.Same(user, result);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingUser_ShouldFail()
    {
        var repository = new FakeUserRepository(null);

        var useCase = new GetCurrentUserProfile(repository);

        var userId = UserId.New();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(
                userId,
                CancellationToken.None));

        Assert.Contains(
            userId.Value.ToString(),
            exception.Message);
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
            if (user?.Id != userId)
            {
                return Task.FromResult<User?>(null);
            }

            return Task.FromResult<User?>(user);
        }

        public Task<User?> GetByExternalIdentityIdAsync(
            string externalIdentityId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<User?>(null);
        }

        public Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public void Add(User user)
        {
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}