import { apiGet } from "@/lib/api/api-client";

import type
{
  OrganizationMember,
} from "@/features/organizations/organization-member";

type GetOrganizationMembersOptions = Readonly<
{
  organizationId: string;
  token: string;
  signal?: AbortSignal;
}>;

export async function getOrganizationMembers(
  options: GetOrganizationMembersOptions
): Promise<OrganizationMember[]>
{
  const response = await apiGet(
    `/organizations/${encodeURIComponent(options.organizationId)}/members`,
    {
      token: options.token,
      signal: options.signal,
    }
  );

  return response.json() as Promise<OrganizationMember[]>;
}