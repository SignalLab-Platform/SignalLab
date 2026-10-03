import { apiGet } from "@/lib/api/api-client";

import type { CurrentUser } from "@/features/current-user/current-user";

type GetCurrentUserOptions = Readonly<
{
  token: string;
  signal?: AbortSignal;
}>;

export async function getCurrentUser(
  options: GetCurrentUserOptions,
): Promise<CurrentUser>
{
  const response = await apiGet(
    "/authentication/session",
    options,
  );

  return response.json() as Promise<CurrentUser>;
}