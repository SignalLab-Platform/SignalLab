"use client";

import { useAuth } from "@clerk/nextjs";
import { useQuery } from "@tanstack/react-query";

import
{
  getOrganizationRoles,
} from "@/features/organizations/get-organization-roles";

export function organizationRolesQueryKey(organizationId: string)
{
  return ["organizations", organizationId, "roles"] as const;
}

export function useOrganizationRoles(
  organizationId: string,
  enabled = true
)
{
  const { getToken, isLoaded, isSignedIn } = useAuth();

  return useQuery(
  {
    queryKey: organizationRolesQueryKey(organizationId),

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

      return getOrganizationRoles(
      {
        organizationId,
        token,
        signal,
      });
    },
  });
}