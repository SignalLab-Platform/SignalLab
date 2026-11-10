import { createApiError } from "@/lib/api/api-error";

type ApiRequestOptions = Readonly<
{
  token: string;
  signal?: AbortSignal;
}>;

function getApiUrl(): string
{
  const apiUrl = process.env.NEXT_PUBLIC_API_URL;

  if (!apiUrl)
  {
    throw new Error(
      "Required configuration 'NEXT_PUBLIC_API_URL' is missing."
    );
  }

  return apiUrl.replace(/\/+$/, "");
}

async function ensureSuccessfulResponse(response: Response): Promise<Response>
{
  if (!response.ok)
  {
    throw await createApiError(response);
  }

  return response;
}

export async function apiGet(
  path: string,
  options: ApiRequestOptions
): Promise<Response>
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
    }
  );

  return ensureSuccessfulResponse(response);
}

export async function apiPatch(
  path: string,
  body: unknown,
  options: ApiRequestOptions
): Promise<Response>
{
  const response = await fetch(
    `${getApiUrl()}${path}`,
    {
      method: "PATCH",
      headers:
      {
        Authorization: `Bearer ${options.token}`,
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(body),
      signal: options.signal,
    }
  );

  return ensureSuccessfulResponse(response);
}

export async function apiPost(
  path: string,
  body: unknown,
  options: ApiRequestOptions
): Promise<Response>
{
  const hasBody = body !== undefined;

  const response = await fetch(
    `${getApiUrl()}${path}`,
    {
      method: "POST",
      headers:
      {
        Authorization: `Bearer ${options.token}`,
        Accept: "application/json",
        ...(hasBody ? { "Content-Type": "application/json" } : {}),
      },
      ...(hasBody ? { body: JSON.stringify(body) } : {}),
      signal: options.signal,
    }
  );

  return ensureSuccessfulResponse(response);
}