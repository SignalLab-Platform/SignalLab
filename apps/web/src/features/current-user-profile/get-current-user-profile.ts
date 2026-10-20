import { apiGet } from "@/lib/api/api-client";

import type
{
  CurrentUserProfile,
} from "@/features/current-user-profile/current-user-profile";

type GetCurrentUserProfileOptions = Readonly<
{
  token: string;
  signal?: AbortSignal;
}>;

export async function getCurrentUserProfile(
  options: GetCurrentUserProfileOptions,
): Promise<CurrentUserProfile>
{
  const response = await apiGet(
    "/users/me/profile",
    options,
  );

  return response.json() as Promise<CurrentUserProfile>;
}