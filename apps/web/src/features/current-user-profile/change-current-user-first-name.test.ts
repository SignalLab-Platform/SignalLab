import { apiPatch } from "@/lib/api/api-client";

import
{
  changeCurrentUserFirstName,
} from "@/features/current-user-profile/change-current-user-first-name";

jest.mock("@/lib/api/api-client");

describe("changeCurrentUserFirstName", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();
  });

  it("changes and returns the current user first name", async () =>
  {
    const response =
    {
      json: jest.fn().mockResolvedValue(
      {
        firstName: "Jimmy",
      }),
    } as unknown as Response;

    jest.mocked(apiPatch).mockResolvedValue(response);

    const result = await changeCurrentUserFirstName(
    {
      token: "test-token",
      firstName: "Jimmy",
    });

    expect(apiPatch).toHaveBeenCalledWith(
      "/users/me/profile/first-name",
      {
        firstName: "Jimmy",
      },
      {
        token: "test-token",
        signal: undefined,
      },
    );

    expect(result).toEqual(
    {
      firstName: "Jimmy",
    });
  });
});