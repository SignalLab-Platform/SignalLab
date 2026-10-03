import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { renderHook, waitFor } from "@testing-library/react";
import { useAuth } from "@clerk/nextjs";

import { getCurrentUser } from "@/features/current-user/get-current-user";
import { useCurrentUser } from "@/features/current-user/use-current-user";

const mockGetToken = jest.fn();

jest.mock("@clerk/nextjs", () =>
({
  useAuth: jest.fn(),
}));

jest.mock("@/features/current-user/get-current-user");

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
  {
    children,
  }: Readonly<
  {
    children: React.ReactNode;
  }>)
  {
    return (
      <QueryClientProvider client={queryClient}>
        {children}
      </QueryClientProvider>
    );
  };
}

describe("useCurrentUser", () =>
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

  it("gets the Clerk token and resolves the current SignalLab user", async () =>
  {
    mockGetToken.mockResolvedValue("test-token");

    const currentUser =
    {
      id: "7e34f695-46df-4c29-9852-030783798531",
      email: "user@example.com",
    };

    jest.mocked(getCurrentUser).mockResolvedValue(currentUser);

    const { result } = renderHook(
      () => useCurrentUser(),
      {
        wrapper: createWrapper(),
      },
    );

    await waitFor(() =>
    {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(mockGetToken).toHaveBeenCalledTimes(1);

    expect(getCurrentUser).toHaveBeenCalledWith(
    {
      token: "test-token",
      signal: expect.any(AbortSignal),
    });

    expect(result.current.data).toEqual(currentUser);
  });

  it("fails when Clerk cannot provide an authentication token", async () =>
  {
    mockGetToken.mockResolvedValue(null);

    const { result } = renderHook(
      () => useCurrentUser(),
      {
        wrapper: createWrapper(),
      },
    );

    await waitFor(() =>
    {
      expect(result.current.isError).toBe(true);
    });

    expect(getCurrentUser).not.toHaveBeenCalled();

    expect(result.current.error).toEqual(
      new Error(
        "Unable to obtain an authentication token for the current user.",
      ),
    );
  });

  it("does not resolve the current user while Clerk authentication is unavailable", () =>
  {
    jest.mocked(useAuth).mockReturnValue(
    {
      getToken: mockGetToken,
      isLoaded: false,
      isSignedIn: undefined,
    } as unknown as ReturnType<typeof useAuth>);

    const { result } = renderHook(
      () => useCurrentUser(),
      {
        wrapper: createWrapper(),
      },
    );

    expect(result.current.fetchStatus).toBe("idle");
    expect(mockGetToken).not.toHaveBeenCalled();
    expect(getCurrentUser).not.toHaveBeenCalled();
  });

  it("does not resolve the current user when no user is signed in", () =>
  {
    jest.mocked(useAuth).mockReturnValue(
    {
      getToken: mockGetToken,
      isLoaded: true,
      isSignedIn: false,
    } as unknown as ReturnType<typeof useAuth>);

    const { result } = renderHook(
      () => useCurrentUser(),
      {
        wrapper: createWrapper(),
      },
    );

    expect(result.current.fetchStatus).toBe("idle");
    expect(mockGetToken).not.toHaveBeenCalled();
    expect(getCurrentUser).not.toHaveBeenCalled();
  });
});
