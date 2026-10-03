import { apiGet } from "@/lib/api/api-client";

import { getCurrentUser } from "@/features/current-user/get-current-user";

jest.mock("@/lib/api/api-client");

describe("getCurrentUser", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();
  });

  it("gets and returns the current SignalLab user", async () =>
  {
    const currentUser =
    {
      id: "7e34f695-46df-4c29-9852-030783798531",
      email: "user@example.com",
    };

    const response =
    {
      json: jest.fn().mockResolvedValue(currentUser),
    } as unknown as Response;

    jest.mocked(apiGet).mockResolvedValue(response);

    const abortController = new AbortController();

    const result = await getCurrentUser(
    {
      token: "test-token",
      signal: abortController.signal,
    });

    expect(apiGet).toHaveBeenCalledWith(
      "/authentication/session",
      {
        token: "test-token",
        signal: abortController.signal,
      },
    );

    expect(response.json).toHaveBeenCalledTimes(1);
    expect(result).toEqual(currentUser);
  });
});