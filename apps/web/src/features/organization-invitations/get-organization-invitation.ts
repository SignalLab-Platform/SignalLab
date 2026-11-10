import { apiGet } from "@/lib/api/api-client";

import type {
  OrganizationInvitation,
} from "@/features/organization-invitations/organization-invitation";

type GetOrganizationInvitationOptions = Readonly<
{
  invitationId: string;
  token: string;
  signal?: AbortSignal;
}>;

export async function getOrganizationInvitation(
  options: GetOrganizationInvitationOptions
): Promise<OrganizationInvitation>
{
  const response = await apiGet(
    `/organization-invitations/${options.invitationId}`,
    {
      token: options.token,
      signal: options.signal,
    }
  );

  return response.json() as Promise<OrganizationInvitation>;
}