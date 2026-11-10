"use client";

import { useAuth } from "@clerk/nextjs";
import { useQuery } from "@tanstack/react-query";

import { getUnreadNotificationCount } from "@/features/notifications/get-unread-notification-count";

export const unreadNotificationCountQueryKey =
  ["current-user", "notifications", "unread-count"] as const;

export function useUnreadNotificationCount()
{
  const { getToken, isLoaded, isSignedIn } = useAuth();

  return useQuery(
  {
    queryKey: unreadNotificationCountQueryKey,

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

      return getUnreadNotificationCount(
      {
        token,
        signal,
      });
    },
  });
}