import { apiPost } from "@/lib/api/api-client";

import type {
  OrganizationInvitationAction,
} from "@/features/organization-invitations/organization-invitation";

type AcceptOrganizationInvitationOptions = Readonly<
{
  invitationId: string;
  token: string;
  signal?: AbortSignal;
}>;

export async function acceptOrganizationInvitation(
  options: AcceptOrganizationInvitationOptions
): Promise<OrganizationInvitationAction>
{
  const response = await apiPost(
    `/organization-invitations/${options.invitationId}/accept`,
    undefined,
    {
      token: options.token,
      signal: options.signal,
    }
  );

  return response.json() as Promise<OrganizationInvitationAction>;
}