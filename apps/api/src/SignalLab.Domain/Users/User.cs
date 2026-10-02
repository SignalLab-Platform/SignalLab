namespace SignalLab.Domain.Users;

public sealed class User
{
    public UserId Id { get; private set; }

    public string ExternalIdentityId { get; private set; }

    public string Email { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private User()
    {
        ExternalIdentityId = null!;
        Email = null!;
    }

    public User(string externalIdentityId, string email, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(externalIdentityId))
        {
            throw new ArgumentException(
                "External identity ID cannot be empty.",
                nameof(externalIdentityId));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException(
                "Email cannot be empty.",
                nameof(email));
        }

        Id = UserId.New();
        ExternalIdentityId = externalIdentityId;
        Email = email.Trim().ToLowerInvariant();
        CreatedAt = createdAt;
    }
}