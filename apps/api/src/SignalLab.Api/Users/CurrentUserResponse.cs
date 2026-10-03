namespace SignalLab.Api.Users;

public sealed record CurrentUserResponse(
    Guid Id,
    string Email);