import { apiPost } from "@/lib/api/api-client";

import type {
  SignalLabNotification,
} from "@/features/notifications/notification";

type MarkNotificationAsReadOptions = Readonly<
{
  notificationId: string;
  token: string;
  signal?: AbortSignal;
}>;

export async function markNotificationAsRead(
  options: MarkNotificationAsReadOptions
): Promise<SignalLabNotification>
{
  const response = await apiPost(
    `/users/me/notifications/${options.notificationId}/read`,
    undefined,
    {
      token: options.token,
      signal: options.signal,
    }
  );

  return response.json() as Promise<SignalLabNotification>;
}