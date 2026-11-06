using SignalLab.Api.Authentication;
using SignalLab.Application.Organizations;

namespace SignalLab.Api.Organizations;

public static class OrganizationEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/organizations")
            .RequireAuthorization();

        group.MapPost(
            "/",
            async (
                CreateOrganizationRequest request,
                CurrentUserContextResolver currentUserContextResolver,
                CreateOrganization createOrganization,
                CancellationToken cancellationToken) =>
            {
                var currentUser = await currentUserContextResolver.ResolveAsync(
                    cancellationToken);

                try
                {
                    var organization = await createOrganization.ExecuteAsync(
                        currentUser.UserId,
                        request.Name!,
                        request.Country!,
                        request.Website,
                        cancellationToken);

                    var response = new CreateOrganizationResponse(
                        organization.Id.Value,
                        organization.Name,
                        organization.Country,
                        organization.Website);

                    return Results.Created(
                        $"/organizations/{organization.Id.Value}",
                        response);
                }
                catch (ArgumentException exception)
                {
                    return Results.BadRequest(
                        new
                        {
                            error = exception.Message
                        });
                }
            })
            .WithName("CreateOrganization");
        
        group.MapGet("/", async (
            CurrentUserContextResolver currentUserContextResolver,
            GetCurrentUserOrganizations getCurrentUserOrganizations,
            CancellationToken cancellationToken) =>
        {
            var currentUser = await currentUserContextResolver.ResolveAsync(
                cancellationToken);

            var organizations = await getCurrentUserOrganizations.ExecuteAsync(
                currentUser.UserId,
                cancellationToken);

            var response = organizations
                .Select(organization => new OrganizationSummaryResponse(
                    organization.Id.Value,
                    organization.Name,
                    organization.Country,
                    organization.Website))
                .ToList();

            return Results.Ok(response);
        })
        .WithName("GetCurrentUserOrganizations");

        return endpoints;
    }
}