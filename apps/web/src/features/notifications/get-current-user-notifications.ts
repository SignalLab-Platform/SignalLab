import { apiGet } from "@/lib/api/api-client";

import type {
  SignalLabNotification,
} from "@/features/notifications/notification";

type GetCurrentUserNotificationsOptions = Readonly<
{
  token: string;
  signal?: AbortSignal;
}>;

export async function getCurrentUserNotifications(
  options: GetCurrentUserNotificationsOptions
): Promise<SignalLabNotification[]>
{
  const response = await apiGet(
    "/users/me/notifications",
    {
      token: options.token,
      signal: options.signal,
    }
  );

  return response.json() as Promise<SignalLabNotification[]>;
}