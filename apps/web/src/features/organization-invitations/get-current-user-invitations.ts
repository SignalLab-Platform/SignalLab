import { apiGet } from "@/lib/api/api-client";

import type {
  CurrentUserInvitation,
} from "@/features/organization-invitations/organization-invitation";

type GetCurrentUserInvitationsOptions = Readonly<
{
  token: string;
  signal?: AbortSignal;
}>;

export async function getCurrentUserInvitations(
  options: GetCurrentUserInvitationsOptions
): Promise<CurrentUserInvitation[]>
{
  const response = await apiGet(
    "/users/me/organization-invitations",
    {
      token: options.token,
      signal: options.signal,
    }
  );

  return response.json() as Promise<CurrentUserInvitation[]>;
}