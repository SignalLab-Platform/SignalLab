using SignalLab.Application.Errors;
using SignalLab.Application.Identity;
using SignalLab.Application.Users;

namespace SignalLab.Api.Authentication;

public sealed class CurrentUserContextResolver
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IUserRepository userRepository;

    public CurrentUserContextResolver(
        IHttpContextAccessor httpContextAccessor,
        IUserRepository userRepository)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.userRepository = userRepository;
    }

    public async Task<CurrentUserContext> ResolveAsync(
        CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            throw new InvalidOperationException(
                "The current HTTP context is unavailable.");
        }

        var externalIdentityId = httpContext.User
            .FindFirst("sub")?
            .Value;

        if (string.IsNullOrWhiteSpace(externalIdentityId))
        {
            throw new AuthenticationRequiredException(
                AuthenticationErrorCodes.MissingExternalIdentity,
                "The authenticated user has no external identity ID.");
        }

        var user = await userRepository.GetByExternalIdentityIdAsync(
            externalIdentityId,
            cancellationToken);

        if (user is null)
        {
            throw new ConflictException(
                AuthenticationErrorCodes.SessionInitializationRequired,
                "The SignalLab session must be initialized before accessing this resource.");
        }

        return new CurrentUserContext(user.Id);
    }
}