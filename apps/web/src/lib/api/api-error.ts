export class ApiError extends Error
{
  readonly status: number;
  readonly code: string | null;
  readonly detail: string | null;

  constructor(status: number, code: string | null, detail: string | null)
  {
    super(`SignalLab API request failed with status ${status}.`);

    this.name = "ApiError";
    this.status = status;
    this.code = code;
    this.detail = detail;
  }
}

export async function createApiError(response: Response): Promise<ApiError>
{
  let code: string | null = null;
  let detail: string | null = null;

  try
  {
    const body: unknown = await response.json();

    if (typeof body === "object" && body !== null)
    {
      const problem = body as Record<string, unknown>;

      if (typeof problem.code === "string")
      {
        code = problem.code;
      }

      if (typeof problem.detail === "string")
      {
        detail = problem.detail;
      }
    }
  }
  catch
  {
    // A failed response does not always contain valid JSON.
  }

  return new ApiError(response.status, code, detail);
}