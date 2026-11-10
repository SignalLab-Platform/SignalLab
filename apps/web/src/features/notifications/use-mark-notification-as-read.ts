"use client";

import { useAuth } from "@clerk/nextjs";
import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  markNotificationAsRead,
} from "@/features/notifications/mark-notification-as-read";

import {
  currentUserNotificationsQueryKey,
} from "@/features/notifications/use-current-user-notifications";

import {
  unreadNotificationCountQueryKey,
} from "@/features/notifications/use-unread-notification-count";

export function useMarkNotificationAsRead()
{
  const { getToken } = useAuth();
  const queryClient = useQueryClient();

  return useMutation(
  {
    mutationFn: async (notificationId: string) =>
    {
      const token = await getToken();

      if (!token)
      {
        throw new Error(
          "Unable to obtain an authentication token for the current user."
        );
      }

      return markNotificationAsRead(
      {
        notificationId,
        token,
      });
    },

    onSuccess: async () =>
    {
      await Promise.all(
      [
        queryClient.invalidateQueries(
        {
          queryKey: currentUserNotificationsQueryKey,
          exact: true,
        }),

        queryClient.invalidateQueries(
        {
          queryKey: unreadNotificationCountQueryKey,
        }),
      ]);
    },
  });
}