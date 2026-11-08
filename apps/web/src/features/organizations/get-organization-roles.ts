import { apiGet } from "@/lib/api/api-client";

import type
{
  OrganizationRole,
} from "@/features/organizations/organization-role";

type GetOrganizationRolesOptions = Readonly<
{
  organizationId: string;
  token: string;
  signal?: AbortSignal;
}>;

export async function getOrganizationRoles(
  options: GetOrganizationRolesOptions
): Promise<OrganizationRole[]>
{
  const response = await apiGet(
    `/organizations/${encodeURIComponent(options.organizationId)}/roles`,
    {
      token: options.token,
      signal: options.signal,
    }
  );

  return response.json() as Promise<OrganizationRole[]>;
}