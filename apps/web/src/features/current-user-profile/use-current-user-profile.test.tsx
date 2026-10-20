import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { renderHook, waitFor } from "@testing-library/react";
import { useAuth } from "@clerk/nextjs";

import
{
  getCurrentUserProfile,
} from "@/features/current-user-profile/get-current-user-profile";
import
{
  useCurrentUserProfile,
} from "@/features/current-user-profile/use-current-user-profile";

const mockGetToken = jest.fn();

jest.mock("@clerk/nextjs", () =>
({
  useAuth: jest.fn(),
}));

jest.mock(
  "@/features/current-user-profile/get-current-user-profile",
);

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

describe("useCurrentUserProfile", () =>
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

  it("gets the Clerk token and resolves the current SignalLab user profile", async () =>
  {
    mockGetToken.mockResolvedValue("test-token");

    const profile =
    {
      id: "7e34f695-46df-4c29-9852-030783798531",
      email: "user@example.com",
      firstName: "Jimmy",
      lastName: "Fromonot",
      language: "English",
      avatarFileId: null,
      avatarUrl: null,
    };

    jest.mocked(getCurrentUserProfile).mockResolvedValue(profile);

    const { result } = renderHook(
      () => useCurrentUserProfile(),
      {
        wrapper: createWrapper(),
      },
    );

    await waitFor(() =>
    {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(mockGetToken).toHaveBeenCalledTimes(1);

    expect(getCurrentUserProfile).toHaveBeenCalledWith(
    {
      token: "test-token",
      signal: expect.any(AbortSignal),
    });

    expect(result.current.data).toEqual(profile);
  });

  it("fails when Clerk cannot provide an authentication token", async () =>
  {
    mockGetToken.mockResolvedValue(null);

    const { result } = renderHook(
      () => useCurrentUserProfile(),
      {
        wrapper: createWrapper(),
      },
    );

    await waitFor(() =>
    {
      expect(result.current.isError).toBe(true);
    });

    expect(getCurrentUserProfile).not.toHaveBeenCalled();
  });

  it("does not resolve the profile while Clerk authentication is unavailable", () =>
  {
    jest.mocked(useAuth).mockReturnValue(
    {
      getToken: mockGetToken,
      isLoaded: false,
      isSignedIn: undefined,
    } as unknown as ReturnType<typeof useAuth>);

    const { result } = renderHook(
      () => useCurrentUserProfile(),
      {
        wrapper: createWrapper(),
      },
    );

    expect(result.current.fetchStatus).toBe("idle");
    expect(mockGetToken).not.toHaveBeenCalled();
    expect(getCurrentUserProfile).not.toHaveBeenCalled();
  });

  it("does not resolve the profile when no user is signed in", () =>
  {
    jest.mocked(useAuth).mockReturnValue(
    {
      getToken: mockGetToken,
      isLoaded: true,
      isSignedIn: false,
    } as unknown as ReturnType<typeof useAuth>);

    const { result } = renderHook(
      () => useCurrentUserProfile(),
      {
        wrapper: createWrapper(),
      },
    );

    expect(result.current.fetchStatus).toBe("idle");
    expect(mockGetToken).not.toHaveBeenCalled();
    expect(getCurrentUserProfile).not.toHaveBeenCalled();
  });
});