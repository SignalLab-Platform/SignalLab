import
{
  apiGet,
  apiPost,
} from "@/lib/api/api-client";

import
{
  ApiError
} from "@/lib/api/api-error";

describe("apiGet", () =>
{
  const originalApiUrl = process.env.NEXT_PUBLIC_API_URL;

  beforeEach(() =>
  {
    process.env.NEXT_PUBLIC_API_URL = "https://api.example.test/";
    global.fetch = jest.fn();
  });

  afterEach(() =>
  {
    process.env.NEXT_PUBLIC_API_URL = originalApiUrl;
    jest.restoreAllMocks();
  });

  it("sends an authenticated GET request to the SignalLab API", async () =>
    {
    const response =
    {
        ok: true,
        status: 200,
    } as Response;

    jest.mocked(global.fetch).mockResolvedValue(response);

    const abortController = new AbortController();

    const result = await apiGet(
        "/authentication/session",
        {
        token: "test-token",
        signal: abortController.signal,
        },
    );

    expect(global.fetch).toHaveBeenCalledWith(
        "https://api.example.test/authentication/session",
        {
        method: "GET",
        headers:
        {
            Authorization: "Bearer test-token",
            Accept: "application/json",
        },
        signal: abortController.signal,
        },
    );

    expect(result).toBe(response);
    });

  it("throws when the API response is unsuccessful", async () =>
    {
    jest.mocked(global.fetch).mockResolvedValue(
        {
        ok: false,
        status: 401,
        } as Response,
    );

    await expect(
        apiGet(
        "/authentication/session",
        {
            token: "test-token",
        },
        ),
    ).rejects.toThrow(
        "SignalLab API request failed with status 401.",
    );
    });

  it("throws when the API URL is not configured", async () =>
  {
    delete process.env.NEXT_PUBLIC_API_URL;

    await expect(
      apiGet(
        "/authentication/session",
        {
          token: "test-token",
        },
      ),
    ).rejects.toThrow(
      "Required configuration 'NEXT_PUBLIC_API_URL' is missing.",
    );

    expect(global.fetch).not.toHaveBeenCalled();
  });
});

describe("apiPost", () =>
{
  const originalApiUrl = process.env.NEXT_PUBLIC_API_URL;

  beforeEach(() =>
  {
    process.env.NEXT_PUBLIC_API_URL = "https://api.example.test/";
    global.fetch = jest.fn();
  });

  afterEach(() =>
  {
    process.env.NEXT_PUBLIC_API_URL = originalApiUrl;
    jest.restoreAllMocks();
  });

  it("sends an authenticated POST request to the SignalLab API", async () =>
  {
    const response =
    {
      ok: true,
      status: 200,
    } as Response;

    jest.mocked(global.fetch).mockResolvedValue(response);

    const abortController = new AbortController();

    const body =
    {
      fileName: "avatar.png",
      contentType: "image/png",
      size: 1024,
    };

    const result = await apiPost(
      "/users/me/avatar/upload",
      body,
      {
        token: "test-token",
        signal: abortController.signal,
      },
    );

    expect(global.fetch).toHaveBeenCalledWith(
      "https://api.example.test/users/me/avatar/upload",
      {
        method: "POST",
        headers:
        {
          Authorization: "Bearer test-token",
          Accept: "application/json",
          "Content-Type": "application/json",
        },
        body: JSON.stringify(body),
        signal: abortController.signal,
      },
    );

    expect(result).toBe(response);
  });

  it("throws when the API response is unsuccessful", async () =>
  {
    jest.mocked(global.fetch).mockResolvedValue(
      {
        ok: false,
        status: 400,
      } as Response,
    );

    await expect(
      apiPost(
        "/users/me/avatar/upload",
        {
          fileName: "avatar.png",
        },
        {
          token: "test-token",
        },
      ),
    ).rejects.toThrow(
      "SignalLab API request failed with status 400.",
    );
  });

  it("preserves the API problem details error code", async () =>
  {
    jest.mocked(global.fetch).mockResolvedValue(
      {
        ok: false,
        status: 409,
        json: jest.fn().mockResolvedValue(
        {
          code: "organization_invitation_already_pending",
          detail: "An invitation is already pending.",
        }),
      } as unknown as Response
    );

    let capturedError: unknown;

    try
    {
      await apiPost(
        "/organizations/organization-1/invitations",
        {
          email: "user@example.com",
          roleId: "role-1",
          duration: 7,
        },
        {
          token: "test-token",
        }
      );
    }
    catch (error)
    {
      capturedError = error;
    }

    expect(capturedError).toBeInstanceOf(ApiError);

    const apiError = capturedError as ApiError;

    expect(apiError.status).toBe(409);
    expect(apiError.code).toBe("organization_invitation_already_pending");
    expect(apiError.detail).toBe("An invitation is already pending.");
  });

  it("sends a bodyless POST without Content-Type", async () =>
  {
    const response =
    {
      ok: true,
      status: 200,
    } as Response;

    jest.mocked(global.fetch).mockResolvedValue(response);

    const result = await apiPost(
      "/organization-invitations/invitation-1/accept",
      undefined,
      {
        token: "test-token",
      }
    );

    expect(global.fetch).toHaveBeenCalledWith(
      "https://api.example.test/organization-invitations/invitation-1/accept",
      {
        method: "POST",
        headers:
        {
          Authorization: "Bearer test-token",
          Accept: "application/json",
        },
        signal: undefined,
      }
    );

    expect(result).toBe(response);
  });
});