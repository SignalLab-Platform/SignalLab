import { apiPost } from "@/lib/api/api-client";
import { createOrganization } from "@/features/organizations/create-organization";

jest.mock("@/lib/api/api-client");

describe("createOrganization", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();
  });

  it("creates an organization and returns the API response", async () =>
  {
    const input =
    {
      name: "Example Studio",
      country: "France",
      website: null,
    };

    const organization =
    {
      id: "7e34f695-46df-4c29-9852-030783798531",
      ...input,
    };

    jest.mocked(apiPost).mockResolvedValue(
    {
      json: async () => organization,
    } as Response);

    const result = await createOrganization(
    {
      token: "test-token",
      input,
    });

    expect(apiPost).toHaveBeenCalledWith(
      "/organizations",
      input,
      {
        token: "test-token",
        signal: undefined,
      }
    );

    expect(result).toEqual(organization);
  });

  it("propagates API errors", async () =>
  {
    jest.mocked(apiPost).mockRejectedValue(
      new Error("API request failed with status 400.")
    );

    await expect(createOrganization(
    {
      token: "test-token",
      input:
      {
        name: "",
        country: "France",
        website: null,
      },
    })).rejects.toThrow("API request failed with status 400.");
  });
});