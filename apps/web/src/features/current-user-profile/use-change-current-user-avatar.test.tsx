import { useAuth } from "@clerk/nextjs";
import
{
  QueryClient,
  QueryClientProvider,
} from "@tanstack/react-query";
import
{
  act,
  renderHook,
  waitFor,
} from "@testing-library/react";

import
{
  changeCurrentUserAvatar,
} from "@/features/current-user-profile/change-current-user-avatar";
import
{
  useChangeCurrentUserAvatar,
} from "@/features/current-user-profile/use-change-current-user-avatar";
import
{
  currentUserProfileQueryKey,
} from "@/features/current-user-profile/use-current-user-profile";

const mockGetToken = jest.fn();

jest.mock("@clerk/nextjs", () =>
({
  useAuth: jest.fn(),
}));

jest.mock(
  "@/features/current-user-profile/change-current-user-avatar",
);

describe("useChangeCurrentUserAvatar", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();

    jest.mocked(useAuth).mockReturnValue(
    {
      getToken: mockGetToken,
    } as unknown as ReturnType<typeof useAuth>);
  });

  it("changes the avatar and invalidates the current user profile", async () =>
  {
    mockGetToken.mockResolvedValue("test-token");

    jest.mocked(changeCurrentUserAvatar)
      .mockResolvedValue(undefined);

    const queryClient = new QueryClient(
    {
      defaultOptions:
      {
        mutations:
        {
          retry: false,
        },
      },
    });

    const invalidateQueries = jest.spyOn(
      queryClient,
      "invalidateQueries",
    );

    function Wrapper(
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
    }

    const { result } = renderHook(
      () => useChangeCurrentUserAvatar(),
      {
        wrapper: Wrapper,
      },
    );

    const file = new File(
      ["avatar-content"],
      "avatar.png",
      {
        type: "image/png",
      },
    );

    await act(async () =>
    {
      await result.current.mutateAsync(file);
    });

    await waitFor(() =>
    {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(mockGetToken).toHaveBeenCalledTimes(1);

    expect(changeCurrentUserAvatar).toHaveBeenCalledWith(
    {
      token: "test-token",
      file,
    });

    expect(invalidateQueries).toHaveBeenCalledWith(
    {
      queryKey: currentUserProfileQueryKey,
    });
  });
});