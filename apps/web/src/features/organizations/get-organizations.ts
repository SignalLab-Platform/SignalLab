import { apiGet } from "@/lib/api/api-client";

import type
{
  Organization,
} from "@/features/organizations/organization";

type GetOrganizationsOptions = Readonly<
{
  token: string;
  signal?: AbortSignal;
}>;

export async function getOrganizations(
  options: GetOrganizationsOptions
): Promise<Organization[]>
{
  const response = await apiGet(
    "/organizations",
    options
  );

  return response.json() as Promise<Organization[]>;
}