import { getOrganizations } from "@/features/organizations/get-organizations";
import { apiGet } from "@/lib/api/api-client";

jest.mock("@/lib/api/api-client", () =>
{
  return {
    apiGet: jest.fn(),
  };
});

const mockedApiGet = jest.mocked(apiGet);

describe("getOrganizations", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();
  });

  it("requests the authenticated user's organizations", async () =>
  {
    const organizations =
    [
      {
        id: "organization-1",
        name: "Example Studio",
        country: "France",
        website: null,
      },
    ];

    mockedApiGet.mockResolvedValue(
    {
      json: async () => organizations,
    } as Response);

    const result = await getOrganizations(
    {
      token: "test-token",
    });

    expect(mockedApiGet).toHaveBeenCalledWith(
      "/organizations",
      {
        token: "test-token",
      }
    );

    expect(result).toEqual(organizations);
  });

  it("returns an empty list when the user has no organizations", async () =>
  {
    mockedApiGet.mockResolvedValue(
    {
      json: async () => [],
    } as Response);

    const result = await getOrganizations(
    {
      token: "test-token",
    });

    expect(result).toEqual([]);
  });

  it("propagates API errors", async () =>
  {
    mockedApiGet.mockRejectedValue(
      new Error("API unavailable")
    );

    await expect(
      getOrganizations(
      {
        token: "test-token",
      })
    ).rejects.toThrow("API unavailable");
  });
});