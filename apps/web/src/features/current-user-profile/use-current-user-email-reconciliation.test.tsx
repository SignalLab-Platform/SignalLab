
import { act, renderHook } from "@testing-library/react";
import { useAuth, useUser } from "@clerk/nextjs";
import { useMutation, useQueryClient } from "@tanstack/react-query";

import
{
  synchronizeCurrentUserEmail,
} from "@/features/current-user-profile/synchronize-current-user-email";
import
{
  currentUserProfileQueryKey,
} from "@/features/current-user-profile/use-current-user-profile";
import
{
  useCurrentUserEmailReconciliation,
} from "@/features/current-user-profile/use-current-user-email-reconciliation";

jest.mock("@clerk/nextjs");
jest.mock("@tanstack/react-query");

jest.mock(
  "@/features/current-user-profile/synchronize-current-user-email",
);

describe("useCurrentUserEmailReconciliation", () =>
{
  const getToken = jest.fn();
  const invalidateQueries = jest.fn();
  const mutateAsync = jest.fn();
  const reset = jest.fn();

  let mutationOptions: Parameters<typeof useMutation>[0];

  beforeEach(() =>
  {
    jest.resetAllMocks();

    jest.mocked(useAuth).mockReturnValue(
    {
      getToken,
    } as unknown as ReturnType<typeof useAuth>);

    jest.mocked(useUser).mockReturnValue(
    {
      user:
      {
        primaryEmailAddress:
        {
          emailAddress: "new@example.com",
        },
      },
    } as ReturnType<typeof useUser>);

    jest.mocked(useQueryClient).mockReturnValue(
    {
      invalidateQueries,
    } as unknown as ReturnType<typeof useQueryClient>);

    jest.mocked(useMutation).mockImplementation((options) =>
    {
      mutationOptions = options;

      return {
        mutateAsync,
        isPending: false,
        isError: false,
        error: null,
        reset,
      } as unknown as ReturnType<typeof useMutation>;
    });

    getToken.mockResolvedValue("test-token");
    invalidateQueries.mockResolvedValue(undefined);

    jest.mocked(synchronizeCurrentUserEmail).mockResolvedValue(
    {
      email: "new@example.com",
    });
  });

  it("does not require reconciliation when emails match", () =>
  {
    const { result } = renderHook(
      () => useCurrentUserEmailReconciliation(),
    );

    expect(
      result.current.needsReconciliation("new@example.com"),
    ).toBe(false);

    expect(synchronizeCurrentUserEmail).not.toHaveBeenCalled();
  });

  it("detects a mismatch between Clerk and SignalLab", () =>
  {
    const { result } = renderHook(
      () => useCurrentUserEmailReconciliation(),
    );

    expect(
      result.current.needsReconciliation("old@example.com"),
    ).toBe(true);
  });

  it("compares emails without case or surrounding whitespace", () =>
  {
    const { result } = renderHook(
      () => useCurrentUserEmailReconciliation(),
    );

    expect(
      result.current.needsReconciliation("  NEW@EXAMPLE.COM  "),
    ).toBe(false);
  });

  it("does not reconcile when the Clerk primary email is unavailable", () =>
  {
    jest.mocked(useUser).mockReturnValue(
    {
      user: null,
    } as ReturnType<typeof useUser>);

    const { result } = renderHook(
      () => useCurrentUserEmailReconciliation(),
    );

    expect(
      result.current.needsReconciliation("old@example.com"),
    ).toBe(false);
  });

  it("synchronizes the primary email using an authentication token", async () =>
  {
    const { result } = renderHook(
      () => useCurrentUserEmailReconciliation(),
    );

    const mutationFn = mutationOptions.mutationFn;

    if (!mutationFn)
    {
      throw new Error("Mutation function is missing.");
    }

    await act(async () =>
    {
      await mutationFn(undefined, {} as Parameters<typeof mutationFn>[1]);
    });

    expect(getToken).toHaveBeenCalled();

    expect(synchronizeCurrentUserEmail).toHaveBeenCalledWith(
    {
      token: "test-token",
    });

    expect(result.current.reconcile).toBe(mutateAsync);
  });

  it("invalidates the profile cache after successful synchronization", async () =>
  {
    renderHook(() => useCurrentUserEmailReconciliation());

    const onSuccess = mutationOptions.onSuccess;

    if (!onSuccess)
    {
      throw new Error("Mutation success callback is missing.");
    }

    await act(async () =>
    {
      await onSuccess(
        { email: "new@example.com" },
        undefined,
        undefined,
        undefined as never,
      );
    });

    expect(invalidateQueries).toHaveBeenCalledWith(
    {
      queryKey: currentUserProfileQueryKey,
    });
  });

  it("rejects synchronization when the authentication token is unavailable", async () =>
  {
    getToken.mockResolvedValue(null);

    renderHook(() => useCurrentUserEmailReconciliation());

    const mutationFn = mutationOptions.mutationFn;

    if (!mutationFn)
    {
      throw new Error("Mutation function is missing.");
    }

    await expect(
      mutationFn(undefined, {} as Parameters<typeof mutationFn>[1]),
    ).rejects.toThrow(
      "Unable to obtain an authentication token for email synchronization.",
    );

    expect(synchronizeCurrentUserEmail).not.toHaveBeenCalled();
  });

  it("exposes the mutation retry and error state", () =>
  {
    jest.mocked(useMutation).mockReturnValue(
    {
      mutateAsync,
      isPending: false,
      isError: true,
      error: new Error("Synchronization failed."),
      reset,
    } as unknown as ReturnType<typeof useMutation>);

    const { result } = renderHook(
      () => useCurrentUserEmailReconciliation(),
    );

    expect(result.current.isError).toBe(true);
    expect(result.current.error?.message).toBe(
      "Synchronization failed.",
    );

    expect(result.current.reconcile).toBe(mutateAsync);
    expect(result.current.reset).toBe(reset);
  });
});
