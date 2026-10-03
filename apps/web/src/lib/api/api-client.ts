type ApiRequestOptions = Readonly<
{
  token: string;
  signal?: AbortSignal;
}>;

function getApiUrl()
{
  const apiUrl = process.env.NEXT_PUBLIC_API_URL;

  if (!apiUrl)
  {
    throw new Error(
      "Required configuration 'NEXT_PUBLIC_API_URL' is missing.",
    );
  }

  return apiUrl.replace(/\/+$/, "");
}

export async function apiGet(
  path: string,
  options: ApiRequestOptions,
)
{
  const response = await fetch(
    `${getApiUrl()}${path}`,
    {
      method: "GET",
      headers:
      {
        Authorization: `Bearer ${options.token}`,
        Accept: "application/json",
      },
      signal: options.signal,
    },
  );

  if (!response.ok)
  {
    throw new Error(
      `SignalLab API request failed with status ${response.status}.`,
    );
  }

  return response;
}
