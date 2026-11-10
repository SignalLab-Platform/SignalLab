"use client";

import { useAuth } from "@clerk/nextjs";
import { useQuery } from "@tanstack/react-query";

import { getOrganizationInvitation } from "@/features/organization-invitations/get-organization-invitation";

export function organizationInvitationQueryKey(invitationId: string)
{
  return ["organization-invitations", invitationId] as const;
}

export function useOrganizationInvitation(invitationId: string)
{
  const { getToken, isLoaded, isSignedIn } = useAuth();

  return useQuery(
  {
    queryKey: organizationInvitationQueryKey(invitationId),

    enabled:
      isLoaded &&
      isSignedIn === true &&
      invitationId.length > 0,

    queryFn: async ({ signal }) =>
    {
      const token = await getToken();

      if (!token)
      {
        throw new Error(
          "Unable to obtain an authentication token for the current user."
        );
      }

      return getOrganizationInvitation(
      {
        invitationId,
        token,
        signal,
      });
    },
  });
}