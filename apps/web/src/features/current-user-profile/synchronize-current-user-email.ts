import { apiPost } from "@/lib/api/api-client";

type SynchronizeCurrentUserEmailOptions = Readonly<
{
  token: string;
  signal?: AbortSignal;
}>;

type SynchronizeCurrentUserEmailResponse =
{
  email: string;
};

export async function synchronizeCurrentUserEmail(
  options: SynchronizeCurrentUserEmailOptions,
): Promise<SynchronizeCurrentUserEmailResponse>
{
  const response = await apiPost(
    "/users/me/profile/email/synchronize",
    {},
    {
      token: options.token,
      signal: options.signal,
    },
  );

  return response.json() as Promise<SynchronizeCurrentUserEmailResponse>;
}