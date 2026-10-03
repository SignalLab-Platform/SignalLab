import { apiGet } from "@/lib/api/api-client";

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