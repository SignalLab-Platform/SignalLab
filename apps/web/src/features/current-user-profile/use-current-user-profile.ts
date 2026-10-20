"use client";

import { useAuth } from "@clerk/nextjs";
import { useQuery } from "@tanstack/react-query";

import
{
  getCurrentUserProfile,
} from "@/features/current-user-profile/get-current-user-profile";

export const currentUserProfileQueryKey =
  ["current-user-profile"] as const;

export function useCurrentUserProfile()
{
  const { getToken, isLoaded, isSignedIn } = useAuth();

  return useQuery(
  {
    queryKey: currentUserProfileQueryKey,
    enabled: isLoaded && isSignedIn === true,
    queryFn: async ({ signal }) =>
    {
      const token = await getToken();

      if (!token)
      {
        throw new Error(
          "Unable to obtain an authentication token for the current user profile.",
        );
      }

      return getCurrentUserProfile(
      {
        token,
        signal,
      });
    },
  });
}