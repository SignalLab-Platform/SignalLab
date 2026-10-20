using SignalLab.Domain.Users;

namespace SignalLab.Api.Authentication;

public sealed record CurrentUserContext(UserId UserId);