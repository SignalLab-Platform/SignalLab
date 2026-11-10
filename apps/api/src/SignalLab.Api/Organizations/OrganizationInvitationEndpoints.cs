using SignalLab.Api.Authentication;
using SignalLab.Application.Errors;
using SignalLab.Application.Organizations;
using SignalLab.Domain.Organizations;
using SignalLab.Domain.Users;

namespace SignalLab.Api.Organizations;

public static class OrganizationInvitationEndpoints
{
    public static IEndpointRouteBuilder MapOrganizationInvitationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var organizationGroup = endpoints
            .MapGroup("/organizations")
            .RequireAuthorization();

        organizationGroup.MapGet(
            "/{organizationId:guid}/invitations",
            async (
                Guid organizationId,
                CurrentUserContextResolver currentUserContextResolver,
                GetOrganizationInvitations getOrganizationInvitations,
                CancellationToken cancellationToken) =>
            {
                var currentUser = await currentUserContextResolver.ResolveAsync(
                    cancellationToken);

                var invitations = await getOrganizationInvitations.ExecuteAsync(
                    new OrganizationId(organizationId),
                    currentUser.UserId,
                    cancellationToken);

                if (invitations is null)
                {
                    return Results.NotFound();
                }

                var response = invitations
                    .Select(OrganizationInvitationResponse.From)
                    .ToList();

                return Results.Ok(response);
            })
            .WithName("GetOrganizationInvitations");

        organizationGroup.MapPost(
            "/{organizationId:guid}/invitations",
            async (
                Guid organizationId,
                CreateOrganizationInvitationRequest request,
                CurrentUserContextResolver currentUserContextResolver,
                CreateOrganizationInvitation createOrganizationInvitation,
                CancellationToken cancellationToken) =>
            {
                var currentUser = await currentUserContextResolver.ResolveAsync(
                    cancellationToken);

                if (request.RoleId == Guid.Empty)
                {
                    throw new ValidationException(
                        OrganizationInvitationErrorCodes.InvalidRoleId,
                        "Role ID cannot be empty.");
                }

                var invitation = await createOrganizationInvitation.ExecuteByEmailAsync(
                    new OrganizationId(organizationId),
                    currentUser.UserId,
                    request.Email,
                    new RoleId(request.RoleId),
                    (OrganizationInvitationDuration)request.Duration,
                    cancellationToken);

                var response = OrganizationInvitationActionResponse.From(
                    invitation);

                return Results.Created(
                    $"/organization-invitations/{invitation.Id.Value}",
                    response);
            })
            .WithName("CreateOrganizationInvitation")
            .RequireRateLimiting(OrganizationInvitationRateLimiting.UserPolicy);

        var invitationGroup = endpoints
            .MapGroup("/organization-invitations")
            .RequireAuthorization();

        invitationGroup.MapGet(
            "/{invitationId:guid}",
            async (
                Guid invitationId,
                CurrentUserContextResolver currentUserContextResolver,
                GetOrganizationInvitation getOrganizationInvitation,
                CancellationToken cancellationToken) =>
            {
                var currentUser = await currentUserContextResolver.ResolveAsync(
                    cancellationToken);

                var invitation = await getOrganizationInvitation.ExecuteAsync(
                    new OrganizationInvitationId(invitationId),
                    currentUser.UserId,
                    cancellationToken);

                if (invitation is null)
                {
                    return Results.NotFound();
                }

                var response = OrganizationInvitationResponse.From(invitation);

                return Results.Ok(response);
            })
            .WithName("GetOrganizationInvitation");

        invitationGroup.MapPost(
            "/{invitationId:guid}/accept",
            async (
                Guid invitationId,
                CurrentUserContextResolver currentUserContextResolver,
                AcceptOrganizationInvitation acceptOrganizationInvitation,
                CancellationToken cancellationToken) =>
            {
                var currentUser = await currentUserContextResolver.ResolveAsync(
                    cancellationToken);

                var invitation = await acceptOrganizationInvitation.ExecuteAsync(
                    new OrganizationInvitationId(invitationId),
                    currentUser.UserId,
                    cancellationToken);

                return Results.Ok(
                    OrganizationInvitationActionResponse.From(invitation));
            })
            .WithName("AcceptOrganizationInvitation");

        invitationGroup.MapPost(
            "/{invitationId:guid}/decline",
            async (
                Guid invitationId,
                CurrentUserContextResolver currentUserContextResolver,
                DeclineOrganizationInvitation declineOrganizationInvitation,
                CancellationToken cancellationToken) =>
            {
                var currentUser = await currentUserContextResolver.ResolveAsync(
                    cancellationToken);

                var invitation = await declineOrganizationInvitation.ExecuteAsync(
                    new OrganizationInvitationId(invitationId),
                    currentUser.UserId,
                    cancellationToken);

                return Results.Ok(
                    OrganizationInvitationActionResponse.From(invitation));
            })
            .WithName("DeclineOrganizationInvitation");

        invitationGroup.MapPost(
            "/{invitationId:guid}/cancel",
            async (
                Guid invitationId,
                CurrentUserContextResolver currentUserContextResolver,
                CancelOrganizationInvitation cancelOrganizationInvitation,
                CancellationToken cancellationToken) =>
            {
                var currentUser = await currentUserContextResolver.ResolveAsync(
                    cancellationToken);

                var invitation = await cancelOrganizationInvitation.ExecuteAsync(
                    new OrganizationInvitationId(invitationId),
                    currentUser.UserId,
                    cancellationToken);

                return Results.Ok(
                    OrganizationInvitationActionResponse.From(invitation));
            })
            .WithName("CancelOrganizationInvitation");

        var currentUserGroup = endpoints
            .MapGroup("/users/me")
            .RequireAuthorization();

        currentUserGroup.MapGet(
            "/organization-invitations",
            async (
                CurrentUserContextResolver currentUserContextResolver,
                GetCurrentUserInvitations getCurrentUserInvitations,
                CancellationToken cancellationToken) =>
            {
                var currentUser = await currentUserContextResolver.ResolveAsync(
                    cancellationToken);

                var invitations = await getCurrentUserInvitations.ExecuteAsync(
                    currentUser.UserId,
                    cancellationToken);

                var response = invitations
                    .Select(CurrentUserInvitationResponse.From)
                    .ToList();

                return Results.Ok(response);
            })
            .WithName("GetCurrentUserInvitations");

        return endpoints;
    }
}