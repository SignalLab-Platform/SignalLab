import { apiGet } from "@/lib/api/api-client";

import type {
  UnreadNotificationCount,
} from "@/features/notifications/notification";

type GetUnreadNotificationCountOptions = Readonly<
{
  token: string;
  signal?: AbortSignal;
}>;

export async function getUnreadNotificationCount(
  options: GetUnreadNotificationCountOptions
): Promise<UnreadNotificationCount>
{
  const response = await apiGet(
    "/users/me/notifications/unread-count",
    {
      token: options.token,
      signal: options.signal,
    }
  );

  return response.json() as Promise<UnreadNotificationCount>;
}