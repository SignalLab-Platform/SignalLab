"use client";

import { useAuth } from "@clerk/nextjs";
import { useQuery } from "@tanstack/react-query";

import { getCurrentUser } from "@/features/current-user/get-current-user";

export const currentUserQueryKey = ["current-user"] as const;

export function useCurrentUser()
{
  const { getToken, isLoaded, isSignedIn } = useAuth();

  return useQuery(
  {
    queryKey: currentUserQueryKey,
    enabled: isLoaded && isSignedIn === true,
    queryFn: async ({ signal }) =>
    {
      const token = await getToken();

      if (!token)
      {
        throw new Error(
          "Unable to obtain an authentication token for the current user.",
        );
      }

      return getCurrentUser(
      {
        token,
        signal,
      });
    },
  });
}