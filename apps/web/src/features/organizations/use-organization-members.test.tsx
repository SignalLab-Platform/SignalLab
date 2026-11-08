import type { ReactNode } from "react";

import { useAuth } from "@clerk/nextjs";

import
{
  QueryClient,
  QueryClientProvider,
} from "@tanstack/react-query";

import
{
  renderHook,
  waitFor,
} from "@testing-library/react";

import { apiGet } from "@/lib/api/api-client";

import
{
  organizationMembersQueryKey,
  useOrganizationMembers,
} from "@/features/organizations/use-organization-members";

jest.mock("@clerk/nextjs");
jest.mock("@/lib/api/api-client");

describe("useOrganizationMembers", () =>
{
  const getToken = jest.fn();

  function createWrapper()
  {
    const queryClient = new QueryClient(
    {
      defaultOptions:
      {
        queries:
        {
          retry: false,
        },
      },
    });

    function Wrapper({ children }: Readonly<{ children: ReactNode }>)
    {
      return (
        <QueryClientProvider client={queryClient}>
          {children}
        </QueryClientProvider>
      );
    }

    return { Wrapper, queryClient };
  }

  function mockAuth(isSignedIn: boolean, isLoaded = true)
  {
    jest.mocked(useAuth).mockReturnValue(
    {
        getToken,
        isLoaded,
        isSignedIn,
    } as unknown as ReturnType<typeof useAuth>);
  }

  beforeEach(() =>
  {
    jest.clearAllMocks();

    getToken.mockResolvedValue("test-token");
    mockAuth(true);

    jest.mocked(apiGet).mockResolvedValue(
    {
      json: async () => [],
    } as Response);
  });

  it("fetches members for the requested Organization", async () =>
  {
    const { Wrapper } = createWrapper();

    const { result } = renderHook(
      () => useOrganizationMembers("organization-alpha"),
      {
        wrapper: Wrapper,
      }
    );

    await waitFor(() =>
    {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(apiGet).toHaveBeenCalledWith(
      "/organizations/organization-alpha/members",
      expect.objectContaining(
      {
        token: "test-token",
        signal: expect.any(AbortSignal),
      })
    );
  });

  it("does not fetch members when signed out", () =>
  {
    mockAuth(false);

    const { Wrapper } = createWrapper();

    const { result } = renderHook(
      () => useOrganizationMembers("organization-alpha"),
      {
        wrapper: Wrapper,
      }
    );

    expect(result.current.fetchStatus).toBe("idle");
    expect(apiGet).not.toHaveBeenCalled();
  });

  it("waits for Clerk to finish loading", () =>
  {
    mockAuth(false, false);

    const { Wrapper } = createWrapper();

    renderHook(
      () => useOrganizationMembers("organization-alpha"),
      {
        wrapper: Wrapper,
      }
    );

    expect(apiGet).not.toHaveBeenCalled();
  });

  it("does not fetch without an Organization ID", () =>
  {
    const { Wrapper } = createWrapper();

    renderHook(
      () => useOrganizationMembers(""),
      {
        wrapper: Wrapper,
      }
    );

    expect(apiGet).not.toHaveBeenCalled();
  });

  it("propagates an authentication token error", async () =>
  {
    getToken.mockResolvedValue(null);

    const { Wrapper } = createWrapper();

    const { result } = renderHook(
      () => useOrganizationMembers("organization-alpha"),
      {
        wrapper: Wrapper,
      }
    );

    await waitFor(() =>
    {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error?.message).toContain(
      "Unable to obtain an authentication token"
    );

    expect(apiGet).not.toHaveBeenCalled();
  });

  it("uses separate cache entries for different Organizations", async () =>
  {
    const { Wrapper, queryClient } = createWrapper();

    const { result, rerender } = renderHook(
      ({ organizationId }) => useOrganizationMembers(organizationId),
      {
        initialProps:
        {
          organizationId: "organization-alpha",
        },
        wrapper: Wrapper,
      }
    );

    await waitFor(() =>
    {
      expect(result.current.isSuccess).toBe(true);
    });

    rerender(
    {
      organizationId: "organization-beta",
    });

    await waitFor(() =>
    {
      expect(apiGet).toHaveBeenCalledWith(
        "/organizations/organization-beta/members",
        expect.any(Object)
      );
    });

    expect(
      queryClient.getQueryState(
        organizationMembersQueryKey("organization-alpha")
      )
    ).toBeDefined();

    expect(
      queryClient.getQueryState(
        organizationMembersQueryKey("organization-beta")
      )
    ).toBeDefined();
  });
});