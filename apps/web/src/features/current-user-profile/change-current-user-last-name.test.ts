import { apiPatch } from "@/lib/api/api-client";

import
{
  changeCurrentUserLastName,
} from "@/features/current-user-profile/change-current-user-last-name";

jest.mock("@/lib/api/api-client");

describe("changeCurrentUserLastName", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();
  });

  it("changes and returns the current user last name", async () =>
  {
    const response =
    {
      json: jest.fn().mockResolvedValue(
      {
        lastName: "Fromonot",
      }),
    } as unknown as Response;

    jest.mocked(apiPatch).mockResolvedValue(response);

    const result = await changeCurrentUserLastName(
    {
      token: "test-token",
      lastName: "Fromonot",
    });

    expect(apiPatch).toHaveBeenCalledWith(
      "/users/me/profile/last-name",
      {
        lastName: "Fromonot",
      },
      {
        token: "test-token",
        signal: undefined,
      },
    );

    expect(result).toEqual(
    {
      lastName: "Fromonot",
    });
  });
});