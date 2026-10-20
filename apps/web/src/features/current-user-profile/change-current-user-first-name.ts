import { apiPatch } from "@/lib/api/api-client";

type ChangeCurrentUserFirstNameOptions = Readonly<
{
  token: string;
  firstName: string | null;
  signal?: AbortSignal;
}>;

type ChangeCurrentUserFirstNameResponse =
{
  firstName: string | null;
};

export async function changeCurrentUserFirstName(
  options: ChangeCurrentUserFirstNameOptions,
): Promise<ChangeCurrentUserFirstNameResponse>
{
  const response = await apiPatch(
    "/users/me/profile/first-name",
    {
      firstName: options.firstName,
    },
    {
      token: options.token,
      signal: options.signal,
    },
  );

  return response.json() as Promise<ChangeCurrentUserFirstNameResponse>;
}