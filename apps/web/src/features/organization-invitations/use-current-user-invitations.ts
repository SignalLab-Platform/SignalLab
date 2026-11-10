"use client";

import { useAuth } from "@clerk/nextjs";
import { useQuery } from "@tanstack/react-query";

import { getCurrentUserInvitations } from "@/features/organization-invitations/get-current-user-invitations";

export const currentUserInvitationsQueryKey =
  ["current-user", "organization-invitations"] as const;

export function useCurrentUserInvitations()
{
  const { getToken, isLoaded, isSignedIn } = useAuth();

  return useQuery(
  {
    queryKey: currentUserInvitationsQueryKey,

    enabled: isLoaded && isSignedIn === true,

    queryFn: async ({ signal }) =>
    {
      const token = await getToken();

      if (!token)
      {
        throw new Error(
          "Unable to obtain an authentication token for the current user."
        );
      }

      return getCurrentUserInvitations(
      {
        token,
        signal,
      });
    },
  });
}