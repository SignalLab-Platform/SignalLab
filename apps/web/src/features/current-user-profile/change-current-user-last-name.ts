import { apiPatch } from "@/lib/api/api-client";

type ChangeCurrentUserLastNameOptions = Readonly<
{
  token: string;
  lastName: string | null;
  signal?: AbortSignal;
}>;

type ChangeCurrentUserLastNameResponse =
{
  lastName: string | null;
};

export async function changeCurrentUserLastName(
  options: ChangeCurrentUserLastNameOptions,
): Promise<ChangeCurrentUserLastNameResponse>
{
  const response = await apiPatch(
    "/users/me/profile/last-name",
    {
      lastName: options.lastName,
    },
    {
      token: options.token,
      signal: options.signal,
    },
  );

  return response.json() as Promise<ChangeCurrentUserLastNameResponse>;
}