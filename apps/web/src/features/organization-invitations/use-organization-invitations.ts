"use client";

import { useAuth } from "@clerk/nextjs";
import { useQuery } from "@tanstack/react-query";

import { getOrganizationInvitations } from "@/features/organization-invitations/get-organization-invitations";

export function organizationInvitationsQueryKey(organizationId: string)
{
  return ["organizations", organizationId, "invitations"] as const;
}

export function useOrganizationInvitations(
  organizationId: string,
  enabled = true
)
{
  const { getToken, isLoaded, isSignedIn } = useAuth();

  return useQuery(
  {
    queryKey: organizationInvitationsQueryKey(organizationId),

    enabled:
      enabled &&
      isLoaded &&
      isSignedIn === true &&
      organizationId.length > 0,

    queryFn: async ({ signal }) =>
    {
      const token = await getToken();

      if (!token)
      {
        throw new Error(
          "Unable to obtain an authentication token for the current user."
        );
      }

      return getOrganizationInvitations(
      {
        organizationId,
        token,
        signal,
      });
    },
  });
}