"use client";

import { useAuth } from "@clerk/nextjs";
import { useQuery } from "@tanstack/react-query";

import
{
  getOrganizationMembers,
} from "@/features/organizations/get-organization-members";

export function organizationMembersQueryKey(
  organizationId: string
)
{
  return ["organizations", organizationId, "members"] as const;
}

export function useOrganizationMembers(organizationId: string)
{
  const { getToken, isLoaded, isSignedIn } = useAuth();

  return useQuery(
  {
    queryKey: organizationMembersQueryKey(organizationId),

    enabled:
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

      return getOrganizationMembers(
      {
        organizationId,
        token,
        signal,
      });
    },
  });
}