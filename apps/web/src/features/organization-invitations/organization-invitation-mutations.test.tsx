import { useAuth } from "@clerk/nextjs";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { act, renderHook } from "@testing-library/react";

import { createOrganizationInvitation } from "./create-organization-invitation";
import { acceptOrganizationInvitation } from "./accept-organization-invitation";
import { declineOrganizationInvitation } from "./decline-organization-invitation";
import { cancelOrganizationInvitation } from "./cancel-organization-invitation";

import { useCreateOrganizationInvitation } from "./use-create-organization-invitation";
import { useAcceptOrganizationInvitation } from "./use-accept-organization-invitation";
import { useDeclineOrganizationInvitation } from "./use-decline-organization-invitation";
import { useCancelOrganizationInvitation } from "./use-cancel-organization-invitation";

import { organizationsQueryKey } from "@/features/organizations/use-organizations";
import { currentUserInvitationsQueryKey } from "./use-current-user-invitations";
import { organizationInvitationQueryKey } from "./use-organization-invitation";
import { organizationInvitationsQueryKey } from "./use-organization-invitations";

import { markNotificationAsRead } from "../notifications/mark-notification-as-read";
import { useMarkNotificationAsRead } from "../notifications/use-mark-notification-as-read";
import { currentUserNotificationsQueryKey } from "../notifications/use-current-user-notifications";
import { unreadNotificationCountQueryKey } from "../notifications/use-unread-notification-count";

jest.mock("@clerk/nextjs", () =>
({
  useAuth: jest.fn(),
}));

jest.mock("./create-organization-invitation");
jest.mock("./accept-organization-invitation");
jest.mock("./decline-organization-invitation");
jest.mock("./cancel-organization-invitation");
jest.mock("../notifications/mark-notification-as-read");

const mockGetToken = jest.fn();

const organizationId = "organization-1";
const invitationId = "invitation-1";
const notificationId = "notification-1";

function createQueryClient(): QueryClient
{
  return new QueryClient(
  {
    defaultOptions:
    {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
}

function createWrapper(queryClient: QueryClient)
{
  return function Wrapper({ children }: Readonly<{ children: React.ReactNode }>)
  {
    return (
      <QueryClientProvider client={queryClient}>
        {children}
      </QueryClientProvider>
    );
  };
}

function expectInvalidatedKeys(
  spy: jest.SpyInstance,
  expectedKeys: readonly (readonly string[])[]
): void
{
  const actualKeys = spy.mock.calls.map((call) => call[0].queryKey);

  expect(actualKeys).toEqual(expectedKeys);
}

describe("Organization invitation and notification mutations", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();

    mockGetToken.mockResolvedValue("test-token");

    jest.mocked(useAuth).mockReturnValue(
    {
      getToken: mockGetToken,
      isLoaded: true,
      isSignedIn: true,
    } as unknown as ReturnType<typeof useAuth>);

    jest.mocked(createOrganizationInvitation).mockResolvedValue(
      { id: invitationId } as Awaited<ReturnType<typeof createOrganizationInvitation>>
    );

    jest.mocked(acceptOrganizationInvitation).mockResolvedValue(
      { id: invitationId } as Awaited<ReturnType<typeof acceptOrganizationInvitation>>
    );

    jest.mocked(declineOrganizationInvitation).mockResolvedValue(
      { id: invitationId } as Awaited<ReturnType<typeof declineOrganizationInvitation>>
    );

    jest.mocked(cancelOrganizationInvitation).mockResolvedValue(
      { id: invitationId } as Awaited<ReturnType<typeof cancelOrganizationInvitation>>
    );

    jest.mocked(markNotificationAsRead).mockResolvedValue(
      { id: notificationId } as Awaited<ReturnType<typeof markNotificationAsRead>>
    );
  });

  it("creates an invitation and invalidates its organization list", async () =>
  {
    const client = createQueryClient();
    const spy = jest.spyOn(client, "invalidateQueries");

    const { result } = renderHook(
      () => useCreateOrganizationInvitation(organizationId),
      { wrapper: createWrapper(client) }
    );

    const input =
    {
      email: "user@example.com",
      roleId: "role-1",
      duration: 7 as const,
    };

    await act(async () =>
    {
      await result.current.mutateAsync(input);
    });

    expect(createOrganizationInvitation).toHaveBeenCalledWith(
    {
      organizationId,
      input,
      token: "test-token",
    });

    expectInvalidatedKeys(spy, [
      organizationInvitationsQueryKey(organizationId),
    ]);
  });

  it("accepts an invitation and refreshes the user's organizations", async () =>
  {
    const client = createQueryClient();
    const spy = jest.spyOn(client, "invalidateQueries");

    const { result } = renderHook(
      () => useAcceptOrganizationInvitation(invitationId),
      { wrapper: createWrapper(client) }
    );

    await act(async () =>
    {
      await result.current.mutateAsync();
    });

    expect(acceptOrganizationInvitation).toHaveBeenCalledWith(
    {
      invitationId,
      token: "test-token",
    });

    expectInvalidatedKeys(spy, [
      organizationInvitationQueryKey(invitationId),
      currentUserInvitationsQueryKey,
      organizationsQueryKey,
    ]);
  });

  it("declines an invitation without refreshing organizations", async () =>
  {
    const client = createQueryClient();
    const spy = jest.spyOn(client, "invalidateQueries");

    const { result } = renderHook(
      () => useDeclineOrganizationInvitation(invitationId),
      { wrapper: createWrapper(client) }
    );

    await act(async () =>
    {
      await result.current.mutateAsync();
    });

    expectInvalidatedKeys(spy, [
      organizationInvitationQueryKey(invitationId),
      currentUserInvitationsQueryKey,
    ]);
  });

  it("cancels an invitation and refreshes its organization list", async () =>
  {
    const client = createQueryClient();
    const spy = jest.spyOn(client, "invalidateQueries");

    const { result } = renderHook(
      () => useCancelOrganizationInvitation(organizationId),
      { wrapper: createWrapper(client) }
    );

    await act(async () =>
    {
      await result.current.mutateAsync(invitationId);
    });

    expect(cancelOrganizationInvitation).toHaveBeenCalledWith(
    {
      invitationId,
      token: "test-token",
    });

    expectInvalidatedKeys(spy, [
      organizationInvitationsQueryKey(organizationId),
      organizationInvitationQueryKey(invitationId),
    ]);
  });

  it("marks a notification as read and refreshes the list and count", async () =>
  {
    const client = createQueryClient();
    const spy = jest.spyOn(client, "invalidateQueries");

    const { result } = renderHook(
      () => useMarkNotificationAsRead(),
      { wrapper: createWrapper(client) }
    );

    await act(async () =>
    {
      await result.current.mutateAsync(notificationId);
    });

    expect(markNotificationAsRead).toHaveBeenCalledWith(
    {
      notificationId,
      token: "test-token",
    });

    expectInvalidatedKeys(spy, [
      currentUserNotificationsQueryKey,
      unreadNotificationCountQueryKey,
    ]);
  });

  it("does not invalidate queries after a failed mutation", async () =>
  {
    const client = createQueryClient();
    const spy = jest.spyOn(client, "invalidateQueries");

    jest.mocked(createOrganizationInvitation).mockRejectedValue(
      new Error("Invitation creation failed.")
    );

    const { result } = renderHook(
      () => useCreateOrganizationInvitation(organizationId),
      { wrapper: createWrapper(client) }
    );

    await act(async () =>
    {
      await expect(result.current.mutateAsync(
      {
        email: "user@example.com",
        roleId: "role-1",
        duration: 7,
      })).rejects.toThrow("Invitation creation failed.");
    });

    expect(spy).not.toHaveBeenCalled();
  });

  it("does not call the API without a Clerk token", async () =>
  {
    mockGetToken.mockResolvedValue(null);

    const client = createQueryClient();

    const { result } = renderHook(
      () => useAcceptOrganizationInvitation(invitationId),
      { wrapper: createWrapper(client) }
    );

    await act(async () =>
    {
      await expect(result.current.mutateAsync()).rejects.toThrow(
        "Unable to obtain an authentication token for the current user."
      );
    });

    expect(acceptOrganizationInvitation).not.toHaveBeenCalled();
  });
});