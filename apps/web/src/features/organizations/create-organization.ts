import { apiPost } from "@/lib/api/api-client";
import type { Organization } from "@/features/organizations/organization";

export type CreateOrganizationInput = Readonly<
{
  name: string;
  country: string;
  website: string | null;
}>;

type CreateOrganizationOptions = Readonly<
{
  token: string;
  input: CreateOrganizationInput;
  signal?: AbortSignal;
}>;

export async function createOrganization(
  options: CreateOrganizationOptions
): Promise<Organization>
{
  const response = await apiPost(
    "/organizations",
    options.input,
    {
      token: options.token,
      signal: options.signal,
    }
  );

  return response.json() as Promise<Organization>;
}