
import { act, renderHook } from "@testing-library/react";
import { useAuth, useReverification, useUser } from "@clerk/nextjs";
import type { EmailAddressResource } from "@clerk/nextjs/types";
import { useQueryClient } from "@tanstack/react-query";

import
{
  promoteClerkEmailToPrimary,
  requestClerkEmailChange,
  verifyClerkEmailChange,
} from "@/features/current-user-profile/change-clerk-email";
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
  useChangeCurrentUserEmail,
} from "@/features/current-user-profile/use-change-current-user-email";

jest.mock("@clerk/nextjs");
jest.mock("@tanstack/react-query");

jest.mock(
  "@/features/current-user-profile/change-clerk-email",
);

jest.mock(
  "@/features/current-user-profile/synchronize-current-user-email",
);

describe("useChangeCurrentUserEmail", () =>
{
  const clerkUser = {} as NonNullable<ReturnType<typeof useUser>["user"]>;

  const emailAddress =
  {
    id: "idn_new_email",
    emailAddress: "new@example.com",
  } as EmailAddressResource;

  const verifiedEmailAddress =
  {
    id: "idn_new_email",
    emailAddress: "new@example.com",
    verification:
    {
      status: "verified",
    },
  } as EmailAddressResource;

  const getToken = jest.fn();
  const invalidateQueries = jest.fn();

  beforeEach(() =>
  {
    jest.resetAllMocks();

    jest.mocked(useUser).mockReturnValue(
    {
      user: clerkUser,
    } as ReturnType<typeof useUser>);

    jest.mocked(useAuth).mockReturnValue(
    {
      getToken,
    } as unknown as ReturnType<typeof useAuth>);

    jest.mocked(useQueryClient).mockReturnValue(
    {
      invalidateQueries,
    } as unknown as ReturnType<typeof useQueryClient>);

    jest.mocked(useReverification).mockImplementation(
      (fetcher) => async (...args) =>
      {
        return await fetcher(...args);
      },
    );

    getToken.mockResolvedValue("test-token");
    invalidateQueries.mockResolvedValue(undefined);

    jest.mocked(requestClerkEmailChange).mockResolvedValue(
    {
      requiresVerification: true,
      emailAddress,
    });

    jest.mocked(verifyClerkEmailChange).mockResolvedValue(
      verifiedEmailAddress,
    );

    jest.mocked(promoteClerkEmailToPrimary).mockResolvedValue(undefined);

    jest.mocked(synchronizeCurrentUserEmail).mockResolvedValue(
    {
      email: "new@example.com",
    });
  });

  it("requests an OTP and enters verification", async () =>
  {
    const { result } = renderHook(() => useChangeCurrentUserEmail());

    await act(async () =>
    {
      await result.current.requestEmailChange("new@example.com");
    });

    expect(requestClerkEmailChange).toHaveBeenCalledWith(
      clerkUser,
      "new@example.com",
    );

    expect(result.current.step).toBe("verification");
    expect(result.current.pendingEmail).toBe("new@example.com");
    expect(result.current.error).toBeNull();
  });
  
  it("promotes an existing verified email without OTP and synchronizes SignalLab", async () =>
  {
    jest.mocked(requestClerkEmailChange).mockResolvedValue(
    {
      requiresVerification: false,
      emailAddress: verifiedEmailAddress,
    });

    const { result } = renderHook(() => useChangeCurrentUserEmail());

    await act(async () =>
    {
      await result.current.requestEmailChange("new@example.com");
    });

    expect(verifyClerkEmailChange).not.toHaveBeenCalled();

    expect(promoteClerkEmailToPrimary).toHaveBeenCalledWith(
      clerkUser,
      verifiedEmailAddress,
    );

    expect(synchronizeCurrentUserEmail).toHaveBeenCalledWith(
    {
      token: "test-token",
    });

    expect(invalidateQueries).toHaveBeenCalledWith(
    {
      queryKey: currentUserProfileQueryKey,
    });

    expect(result.current.step).toBe("completed");
    expect(result.current.pendingEmail).toBeNull();
    expect(result.current.error).toBeNull();
  });

  it("retries direct promotion without requesting OTP", async () =>
  {
    jest.mocked(requestClerkEmailChange).mockResolvedValue(
    {
      requiresVerification: false,
      emailAddress: verifiedEmailAddress,
    });

    jest.mocked(promoteClerkEmailToPrimary)
      .mockRejectedValueOnce(new Error("Clerk promotion failed."))
      .mockResolvedValueOnce(undefined);

    const { result } = renderHook(() => useChangeCurrentUserEmail());

    await act(async () =>
    {
      await result.current.requestEmailChange("new@example.com");
    });

    expect(result.current.step).toBe("promotion_failed");
    expect(result.current.error).toBe("Clerk promotion failed.");
    expect(result.current.pendingEmail).toBe("new@example.com");

    expect(synchronizeCurrentUserEmail).not.toHaveBeenCalled();

    await act(async () =>
    {
      await result.current.retryPromotion();
    });

    expect(result.current.step).toBe("completed");
    expect(result.current.error).toBeNull();

    expect(requestClerkEmailChange).toHaveBeenCalledTimes(1);
    expect(verifyClerkEmailChange).not.toHaveBeenCalled();
    expect(promoteClerkEmailToPrimary).toHaveBeenCalledTimes(2);
    expect(synchronizeCurrentUserEmail).toHaveBeenCalledTimes(1);
  });

  it("retries synchronization after direct promotion without repeating Clerk operations", async () =>
  {
    jest.mocked(requestClerkEmailChange).mockResolvedValue(
    {
      requiresVerification: false,
      emailAddress: verifiedEmailAddress,
    });

    jest.mocked(synchronizeCurrentUserEmail)
      .mockRejectedValueOnce(new Error("SignalLab API unavailable."))
      .mockResolvedValueOnce(
      {
        email: "new@example.com",
      });

    const { result } = renderHook(() => useChangeCurrentUserEmail());

    await act(async () =>
    {
      await result.current.requestEmailChange("new@example.com");
    });

    expect(result.current.step).toBe("synchronization_failed");
    expect(result.current.error).toBe("SignalLab API unavailable.");

    await act(async () =>
    {
      await result.current.retrySynchronization();
    });

    expect(result.current.step).toBe("completed");
    expect(result.current.error).toBeNull();

    expect(requestClerkEmailChange).toHaveBeenCalledTimes(1);
    expect(verifyClerkEmailChange).not.toHaveBeenCalled();
    expect(promoteClerkEmailToPrimary).toHaveBeenCalledTimes(1);
    expect(synchronizeCurrentUserEmail).toHaveBeenCalledTimes(2);
  });

  it("verifies the OTP, promotes the email and synchronizes SignalLab", async () =>
  {
    const { result } = renderHook(() => useChangeCurrentUserEmail());

    await act(async () =>
    {
      await result.current.requestEmailChange("new@example.com");
    });

    await act(async () =>
    {
      await result.current.verifyEmailChange("123456");
    });

    expect(verifyClerkEmailChange).toHaveBeenCalledWith(
      emailAddress,
      "123456",
    );

    expect(promoteClerkEmailToPrimary).toHaveBeenCalledWith(
      clerkUser,
      verifiedEmailAddress,
    );

    expect(synchronizeCurrentUserEmail).toHaveBeenCalledWith(
    {
      token: "test-token",
    });

    expect(invalidateQueries).toHaveBeenCalledWith(
    {
      queryKey: currentUserProfileQueryKey,
    });

    expect(result.current.step).toBe("completed");
    expect(result.current.pendingEmail).toBeNull();
    expect(result.current.error).toBeNull();
  });

  it("stays in verification when Clerk rejects the OTP", async () =>
  {
    jest.mocked(verifyClerkEmailChange).mockRejectedValue(
      new Error("Invalid verification code."),
    );

    const { result } = renderHook(() => useChangeCurrentUserEmail());

    await act(async () =>
    {
      await result.current.requestEmailChange("new@example.com");
    });

    await act(async () =>
    {
      await result.current.verifyEmailChange("000000");
    });

    expect(result.current.step).toBe("verification");
    expect(result.current.error).toBe("Invalid verification code.");

    expect(promoteClerkEmailToPrimary).not.toHaveBeenCalled();
    expect(synchronizeCurrentUserEmail).not.toHaveBeenCalled();
  });

  it("retries promotion without repeating OTP verification", async () =>
  {
    jest.mocked(promoteClerkEmailToPrimary)
      .mockRejectedValueOnce(new Error("Clerk promotion failed."))
      .mockResolvedValueOnce(undefined);

    const { result } = renderHook(() => useChangeCurrentUserEmail());

    await act(async () =>
    {
      await result.current.requestEmailChange("new@example.com");
    });

    await act(async () =>
    {
      await result.current.verifyEmailChange("123456");
    });

    expect(result.current.step).toBe("promotion_failed");
    expect(result.current.error).toBe("Clerk promotion failed.");

    expect(synchronizeCurrentUserEmail).not.toHaveBeenCalled();

    await act(async () =>
    {
      await result.current.retryPromotion();
    });

    expect(result.current.step).toBe("completed");
    expect(result.current.error).toBeNull();

    expect(verifyClerkEmailChange).toHaveBeenCalledTimes(1);
    expect(promoteClerkEmailToPrimary).toHaveBeenCalledTimes(2);
    expect(synchronizeCurrentUserEmail).toHaveBeenCalledTimes(1);
  });

  it("retries synchronization without repeating Clerk operations", async () =>
  {
    jest.mocked(synchronizeCurrentUserEmail)
      .mockRejectedValueOnce(new Error("SignalLab API unavailable."))
      .mockResolvedValueOnce(
      {
        email: "new@example.com",
      });

    const { result } = renderHook(() => useChangeCurrentUserEmail());

    await act(async () =>
    {
      await result.current.requestEmailChange("new@example.com");
    });

    await act(async () =>
    {
      await result.current.verifyEmailChange("123456");
    });

    expect(result.current.step).toBe("synchronization_failed");
    expect(result.current.error).toBe("SignalLab API unavailable.");

    await act(async () =>
    {
      await result.current.retrySynchronization();
    });

    expect(result.current.step).toBe("completed");
    expect(result.current.error).toBeNull();

    expect(verifyClerkEmailChange).toHaveBeenCalledTimes(1);
    expect(promoteClerkEmailToPrimary).toHaveBeenCalledTimes(1);
    expect(synchronizeCurrentUserEmail).toHaveBeenCalledTimes(2);
  });

  it("does not synchronize when Clerk email creation fails", async () =>
  {
    jest.mocked(requestClerkEmailChange).mockRejectedValue(
      new Error("Email already exists."),
    );

    const { result } = renderHook(() => useChangeCurrentUserEmail());

    await act(async () =>
    {
      await result.current.requestEmailChange("new@example.com");
    });

    expect(result.current.step).toBe("idle");
    expect(result.current.error).toBe("Email already exists.");

    expect(verifyClerkEmailChange).not.toHaveBeenCalled();
    expect(promoteClerkEmailToPrimary).not.toHaveBeenCalled();
    expect(synchronizeCurrentUserEmail).not.toHaveBeenCalled();
  });

  it("resets the workflow before Clerk promotion", async () =>
  {
    const { result } = renderHook(() => useChangeCurrentUserEmail());

    await act(async () =>
    {
      await result.current.requestEmailChange("new@example.com");
    });

    act(() =>
    {
      result.current.reset();
    });

    expect(result.current.step).toBe("idle");
    expect(result.current.pendingEmail).toBeNull();
    expect(result.current.error).toBeNull();
  });

  it("preserves the workflow when promotion has failed", async () =>
  {
    jest.mocked(promoteClerkEmailToPrimary).mockRejectedValue(
      new Error("Clerk promotion failed."),
    );

    const { result } = renderHook(() => useChangeCurrentUserEmail());

    await act(async () =>
    {
      await result.current.requestEmailChange("new@example.com");
    });

    await act(async () =>
    {
      await result.current.verifyEmailChange("123456");
    });

    expect(result.current.step).toBe("promotion_failed");

    act(() =>
    {
      result.current.reset();
    });

    expect(result.current.step).toBe("promotion_failed");
    expect(result.current.pendingEmail).toBe("new@example.com");
  });

  it("preserves the workflow when synchronization has failed", async () =>
  {
    jest.mocked(synchronizeCurrentUserEmail).mockRejectedValue(
      new Error("Synchronization failed."),
    );

    const { result } = renderHook(() => useChangeCurrentUserEmail());

    await act(async () =>
    {
      await result.current.requestEmailChange("new@example.com");
    });

    await act(async () =>
    {
      await result.current.verifyEmailChange("123456");
    });

    expect(result.current.step).toBe("synchronization_failed");

    act(() =>
    {
      result.current.reset();
    });

    expect(result.current.step).toBe("synchronization_failed");
    expect(result.current.pendingEmail).toBe("new@example.com");
  });
});
