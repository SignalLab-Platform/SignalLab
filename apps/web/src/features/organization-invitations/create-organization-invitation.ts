import { apiPost } from "@/lib/api/api-client";

import type {
  CreateOrganizationInvitationInput,
  OrganizationInvitationAction,
} from "@/features/organization-invitations/organization-invitation";

type CreateOrganizationInvitationOptions = Readonly<
{
  organizationId: string;
  input: CreateOrganizationInvitationInput;
  token: string;
  signal?: AbortSignal;
}>;

export async function createOrganizationInvitation(
  options: CreateOrganizationInvitationOptions
): Promise<OrganizationInvitationAction>
{
  const response = await apiPost(
    `/organizations/${options.organizationId}/invitations`,
    options.input,
    {
      token: options.token,
      signal: options.signal,
    }
  );

  return response.json() as Promise<OrganizationInvitationAction>;
}