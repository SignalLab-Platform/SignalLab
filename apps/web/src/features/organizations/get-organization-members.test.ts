import { apiGet } from "@/lib/api/api-client";

import
{
  getOrganizationMembers,
} from "@/features/organizations/get-organization-members";

jest.mock("@/lib/api/api-client");

describe("getOrganizationMembers", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();
  });

  it("fetches members for the requested Organization", async () =>
  {
    const members =
    [
      {
        id: "membership-one",
        userId: "user-one",
        firstName: "Alice",
        lastName: "Martin",
        email: "alice@example.com",
        roleId: "role-owner",
        roleName: "Owner",
        status: "Active",
        joinedAt: "2026-01-15T10:00:00Z",
        removedAt: null,
      },
    ];

    jest.mocked(apiGet).mockResolvedValue(
    {
      json: async () => members,
    } as Response);

    const result = await getOrganizationMembers(
    {
      organizationId: "organization-alpha",
      token: "test-token",
    });

    expect(apiGet).toHaveBeenCalledWith(
      "/organizations/organization-alpha/members",
      {
        token: "test-token",
        signal: undefined,
      }
    );

    expect(result).toEqual(members);
  });

  it("forwards the AbortSignal", async () =>
  {
    const controller = new AbortController();

    jest.mocked(apiGet).mockResolvedValue(
    {
      json: async () => [],
    } as Response);

    await getOrganizationMembers(
    {
      organizationId: "organization-alpha",
      token: "test-token",
      signal: controller.signal,
    });

    expect(apiGet).toHaveBeenCalledWith(
      "/organizations/organization-alpha/members",
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

    await expect(getOrganizationMembers(
    {
      organizationId: "organization-alpha",
      token: "test-token",
    })).rejects.toThrow(
      "API request failed with status 403."
    );
  });
});