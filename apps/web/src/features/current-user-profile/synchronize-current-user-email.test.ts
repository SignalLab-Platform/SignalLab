import { apiPost } from "@/lib/api/api-client";

import
{
  synchronizeCurrentUserEmail,
} from "@/features/current-user-profile/synchronize-current-user-email";

jest.mock("@/lib/api/api-client");

describe("synchronizeCurrentUserEmail", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();
  });

  it("synchronizes and returns the current user email", async () =>
  {
    const response =
    {
      json: jest.fn().mockResolvedValue(
      {
        email: "new@example.com",
      }),
    } as unknown as Response;

    jest.mocked(apiPost).mockResolvedValue(response);

    const result = await synchronizeCurrentUserEmail(
    {
      token: "test-token",
    });

    expect(apiPost).toHaveBeenCalledWith(
      "/users/me/profile/email/synchronize",
      {},
      {
        token: "test-token",
        signal: undefined,
      },
    );

    expect(result).toEqual(
    {
      email: "new@example.com",
    });
  });
});