import { apiPost } from "@/lib/api/api-client";

import type {
  OrganizationInvitationAction,
} from "@/features/organization-invitations/organization-invitation";

type CancelOrganizationInvitationOptions = Readonly<
{
  invitationId: string;
  token: string;
  signal?: AbortSignal;
}>;

export async function cancelOrganizationInvitation(
  options: CancelOrganizationInvitationOptions
): Promise<OrganizationInvitationAction>
{
  const response = await apiPost(
    `/organization-invitations/${options.invitationId}/cancel`,
    undefined,
    {
      token: options.token,
      signal: options.signal,
    }
  );

  return response.json() as Promise<OrganizationInvitationAction>;
}