import { apiPost } from "@/lib/api/api-client";

import type {
  OrganizationInvitationAction,
} from "@/features/organization-invitations/organization-invitation";

type DeclineOrganizationInvitationOptions = Readonly<
{
  invitationId: string;
  token: string;
  signal?: AbortSignal;
}>;

export async function declineOrganizationInvitation(
  options: DeclineOrganizationInvitationOptions
): Promise<OrganizationInvitationAction>
{
  const response = await apiPost(
    `/organization-invitations/${options.invitationId}/decline`,
    undefined,
    {
      token: options.token,
      signal: options.signal,
    }
  );

  return response.json() as Promise<OrganizationInvitationAction>;
}