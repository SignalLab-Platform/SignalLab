import { apiGet } from "@/lib/api/api-client";

import
{
  getCurrentUserProfile,
} from "@/features/current-user-profile/get-current-user-profile";

jest.mock("@/lib/api/api-client");

describe("getCurrentUserProfile", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();
  });

  it("gets and returns the current SignalLab user profile", async () =>
  {
    const profile =
    {
      id: "7e34f695-46df-4c29-9852-030783798531",
      email: "user@example.com",
      firstName: "Jimmy",
      lastName: "Fromonot",
      language: "English",
      avatarFileId: null,
    };

    const response =
    {
      json: jest.fn().mockResolvedValue(profile),
    } as unknown as Response;

    jest.mocked(apiGet).mockResolvedValue(response);

    const abortController = new AbortController();

    const result = await getCurrentUserProfile(
    {
      token: "test-token",
      signal: abortController.signal,
    });

    expect(apiGet).toHaveBeenCalledWith(
      "/users/me/profile",
      {
        token: "test-token",
        signal: abortController.signal,
      },
    );

    expect(response.json).toHaveBeenCalledTimes(1);
    expect(result).toEqual(profile);
  });
});