import { apiGet } from "@/lib/api/api-client";

import type {
  OrganizationInvitation,
} from "@/features/organization-invitations/organization-invitation";

type GetOrganizationInvitationsOptions = Readonly<
{
  organizationId: string;
  token: string;
  signal?: AbortSignal;
}>;

export async function getOrganizationInvitations(
  options: GetOrganizationInvitationsOptions
): Promise<OrganizationInvitation[]>
{
  const response = await apiGet(
    `/organizations/${options.organizationId}/invitations`,
    {
      token: options.token,
      signal: options.signal,
    }
  );

  return response.json() as Promise<OrganizationInvitation[]>;
}