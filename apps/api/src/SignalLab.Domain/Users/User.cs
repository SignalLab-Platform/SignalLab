using SignalLab.Domain.Files;

namespace SignalLab.Domain.Users;

public sealed class User
{
    public UserId Id { get; private set; }

    public string ExternalIdentityId { get; private set; }

    public string Email { get; private set; }

    public string? FirstName { get; private set; }

    public string? LastName { get; private set; }

    public UserLanguage Language { get; private set; }

    public StoredFileId? AvatarFileId { get; private set; }

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
        Language = UserLanguage.English;
        CreatedAt = createdAt;
    }

    public void ChangeFirstName(string? firstName)
    {
        FirstName = NormalizeOptionalText(firstName);
    }

    public void ChangeLastName(string? lastName)
    {
        LastName = NormalizeOptionalText(lastName);
    }

    public void ChangeLanguage(UserLanguage language)
    {
        Language = language;
    }

    public void ChangeAvatar(StoredFileId avatarFileId)
    {
        AvatarFileId = avatarFileId;
    }

    public void RemoveAvatar()
    {
        AvatarFileId = null;
    }

    public void ChangeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException(
                "Email cannot be empty.",
                nameof(email));
        }

        Email = email.Trim().ToLowerInvariant();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}