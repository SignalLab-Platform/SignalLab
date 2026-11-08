using SignalLab.Api.Authentication;
using SignalLab.Application.Organizations;
using SignalLab.Domain.Organizations;

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

        group.MapGet(
            "/{organizationId:guid}",
            async (
                Guid organizationId,
                CurrentUserContextResolver currentUserContextResolver,
                GetOrganization getOrganization,
                CancellationToken cancellationToken) =>
            {
                var currentUser = await currentUserContextResolver.ResolveAsync(
                    cancellationToken);

                var organization = await getOrganization.ExecuteAsync(
                    new OrganizationId(organizationId),
                    currentUser.UserId,
                    cancellationToken);

                if (organization is null)
                {
                    return Results.NotFound();
                }

                var response = new OrganizationDetailsResponse(
                    organization.Id.Value,
                    organization.Name,
                    organization.Country,
                    organization.Website,
                    organization.CreatedAt,
                    organization.UpdatedAt);

                return Results.Ok(response);
            })
            .WithName("GetOrganization");
        
        group.MapGet(
            "/{organizationId:guid}/members",
             async (
                Guid organizationId,
                CurrentUserContextResolver currentUserContextResolver,
                GetOrganizationMembers getOrganizationMembers,
                CancellationToken cancellationToken) =>
            {
                var currentUser = await currentUserContextResolver.ResolveAsync(
                    cancellationToken);

                var members = await getOrganizationMembers.ExecuteAsync(
                    new OrganizationId(organizationId),
                    currentUser.UserId,
                    cancellationToken);

                if (members is null)
                {
                    return Results.NotFound();
                }

                var response = members
                    .Select(member => new OrganizationMemberResponse(
                        member.Id.Value,
                        member.UserId.Value,
                        member.FirstName,
                        member.LastName,
                        member.Email,
                        member.RoleId.Value,
                        member.RoleName,
                        member.Status.ToString(),
                        member.JoinedAt,
                        member.RemovedAt))
                    .ToList();

                return Results.Ok(response);
            })
            .WithName("GetOrganizationMembers");

        group.MapGet(
            "/{organizationId:guid}/roles",
            async (
                Guid organizationId,
                CurrentUserContextResolver currentUserContextResolver,
                GetOrganizationRoles getOrganizationRoles,
                CancellationToken cancellationToken) =>
            {
                var currentUser = await currentUserContextResolver.ResolveAsync(
                    cancellationToken);

                var roles = await getOrganizationRoles.ExecuteAsync(
                    new OrganizationId(organizationId),
                    currentUser.UserId,
                    cancellationToken);

                if (roles is null)
                {
                    return Results.NotFound();
                }

                var response = roles
                    .Select(role => new OrganizationRoleResponse(
                        role.Id.Value,
                        role.Name,
                        role.Description,
                        role.IsOwner,
                        role.Permissions
                            .OrderBy(permission => (int)permission)
                            .Select(permission => permission.ToString())
                            .ToList(),
                        role.CreatedAt,
                        role.UpdatedAt))
                    .ToList();

                return Results.Ok(response);
            })
            .WithName("GetOrganizationRoles");

        return endpoints;
    }
}