"use client";

import { useAuth } from "@clerk/nextjs";
import { useQuery } from "@tanstack/react-query";

import
{
  getOrganizations,
} from "@/features/organizations/get-organizations";

export const organizationsQueryKey = ["organizations"] as const;

export function useOrganizations()
{
  const { getToken, isLoaded, isSignedIn } = useAuth();

  return useQuery(
  {
    queryKey: organizationsQueryKey,

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

      return getOrganizations(
      {
        token,
        signal,
      });
    },
  });
}