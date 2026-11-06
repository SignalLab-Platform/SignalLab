import { useAuth } from "@clerk/nextjs";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { renderHook, waitFor } from "@testing-library/react";

import { getOrganizations } from "@/features/organizations/get-organizations";
import { useOrganizations } from "@/features/organizations/use-organizations";

const mockGetToken = jest.fn();

jest.mock("@clerk/nextjs", () =>
({
  useAuth: jest.fn(),
}));

jest.mock("@/features/organizations/get-organizations");

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

  return function Wrapper(
    { children }: Readonly<{ children: React.ReactNode }>
  )
  {
    return (
      <QueryClientProvider client={queryClient}>
        {children}
      </QueryClientProvider>
    );
  };
}

describe("useOrganizations", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();

    jest.mocked(useAuth).mockReturnValue(
    {
      getToken: mockGetToken,
      isLoaded: true,
      isSignedIn: true,
    } as unknown as ReturnType<typeof useAuth>);
  });

  it("retrieves the authenticated user's organizations", async () =>
  {
    mockGetToken.mockResolvedValue("test-token");

    const organizations =
    [
      {
        id: "organization-1",
        name: "Example Studio",
        country: "France",
        website: null,
      },
    ];

    jest.mocked(getOrganizations).mockResolvedValue(organizations);

    const { result } = renderHook(
      () => useOrganizations(),
      {
        wrapper: createWrapper(),
      }
    );

    await waitFor(() =>
    {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(mockGetToken).toHaveBeenCalledTimes(1);

    expect(getOrganizations).toHaveBeenCalledWith(
    {
      token: "test-token",
      signal: expect.any(AbortSignal),
    });

    expect(result.current.data).toEqual(organizations);
  });

  it("supports an empty organizations list", async () =>
  {
    mockGetToken.mockResolvedValue("test-token");
    jest.mocked(getOrganizations).mockResolvedValue([]);

    const { result } = renderHook(
      () => useOrganizations(),
      {
        wrapper: createWrapper(),
      }
    );

    await waitFor(() =>
    {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(result.current.data).toEqual([]);
  });

  it("fails when Clerk cannot provide an authentication token", async () =>
  {
    mockGetToken.mockResolvedValue(null);

    const { result } = renderHook(
      () => useOrganizations(),
      {
        wrapper: createWrapper(),
      }
    );

    await waitFor(() =>
    {
      expect(result.current.isError).toBe(true);
    });

    expect(getOrganizations).not.toHaveBeenCalled();

    expect(result.current.error).toEqual(
      new Error(
        "Unable to obtain an authentication token for the current user."
      )
    );
  });

  it("does not fetch organizations before Clerk is loaded", () =>
  {
    jest.mocked(useAuth).mockReturnValue(
    {
      getToken: mockGetToken,
      isLoaded: false,
      isSignedIn: undefined,
    } as unknown as ReturnType<typeof useAuth>);

    const { result } = renderHook(
      () => useOrganizations(),
      {
        wrapper: createWrapper(),
      }
    );

    expect(result.current.fetchStatus).toBe("idle");
    expect(getOrganizations).not.toHaveBeenCalled();
  });

  it("does not fetch organizations when signed out", () =>
  {
    jest.mocked(useAuth).mockReturnValue(
    {
      getToken: mockGetToken,
      isLoaded: true,
      isSignedIn: false,
    } as unknown as ReturnType<typeof useAuth>);

    const { result } = renderHook(
      () => useOrganizations(),
      {
        wrapper: createWrapper(),
      }
    );

    expect(result.current.fetchStatus).toBe("idle");
    expect(getOrganizations).not.toHaveBeenCalled();
  });
});