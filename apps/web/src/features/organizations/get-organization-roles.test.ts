import { apiGet } from "@/lib/api/api-client";

import
{
  getOrganizationRoles,
} from "@/features/organizations/get-organization-roles";

jest.mock("@/lib/api/api-client");

describe("getOrganizationRoles", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();
  });

  it("fetches roles for the requested Organization", async () =>
  {
    const roles =
    [
      {
        id: "role-owner",
        name: "Owner",
        description: "Organization owner",
        isOwner: true,
        permissions: ["ManageOrganization", "ManageMembers"],
        createdAt: "2026-01-01T00:00:00Z",
        updatedAt: "2026-01-01T00:00:00Z",
      },
    ];

    jest.mocked(apiGet).mockResolvedValue(
    {
      json: async () => roles,
    } as Response);

    const result = await getOrganizationRoles(
    {
      organizationId: "organization-alpha",
      token: "test-token",
    });

    expect(apiGet).toHaveBeenCalledWith(
      "/organizations/organization-alpha/roles",
      {
        token: "test-token",
        signal: undefined,
      }
    );

    expect(result).toEqual(roles);
  });

  it("forwards the AbortSignal", async () =>
  {
    const controller = new AbortController();

    jest.mocked(apiGet).mockResolvedValue(
    {
      json: async () => [],
    } as Response);

    await getOrganizationRoles(
    {
      organizationId: "organization-alpha",
      token: "test-token",
      signal: controller.signal,
    });

    expect(apiGet).toHaveBeenCalledWith(
      "/organizations/organization-alpha/roles",
      {
        token: "test-token",
        signal: controller.signal,
      }
    );
  });

  it("propagates API errors", async () =>
  {
    jest.mocked(apiGet).mockRejectedValue(
      new Error("API request failed with status 403.")
    );

    await expect(getOrganizationRoles(
    {
      organizationId: "organization-alpha",
      token: "test-token",
    })).rejects.toThrow(
      "API request failed with status 403."
    );
  });
});