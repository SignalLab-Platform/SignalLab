"use client";

import { useAuth } from "@clerk/nextjs";
import { useQuery } from "@tanstack/react-query";

import { getCurrentUserNotifications } from "@/features/notifications/get-current-user-notifications";

export const currentUserNotificationsQueryKey =
  ["current-user", "notifications"] as const;

export function useCurrentUserNotifications(enabled = true)
{
  const { getToken, isLoaded, isSignedIn } = useAuth();

  return useQuery(
  {
    queryKey: currentUserNotificationsQueryKey,

    enabled:
      enabled &&
      isLoaded &&
      isSignedIn === true,

    queryFn: async ({ signal }) =>
    {
      const token = await getToken();

      if (!token)
      {
        throw new Error(
          "Unable to obtain an authentication token for the current user."
        );
      }

      return getCurrentUserNotifications(
      {
        token,
        signal,
      });
    },
  });
}