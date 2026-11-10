import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { usePathname, useRouter } from "next/navigation";

import { NotificationTrigger } from "./notification-trigger";

import {
  useCurrentUserNotifications,
} from "@/features/notifications/use-current-user-notifications";

import {
  useUnreadNotificationCount,
} from "@/features/notifications/use-unread-notification-count";

import {
  useMarkNotificationAsRead,
} from "@/features/notifications/use-mark-notification-as-read";

import type {
  SignalLabNotification,
} from "@/features/notifications/notification";

jest.mock("next/navigation", () =>
({
  useRouter: jest.fn(),
  usePathname: jest.fn(),
}));

jest.mock("@/features/notifications/use-current-user-notifications");
jest.mock("@/features/notifications/use-unread-notification-count");
jest.mock("@/features/notifications/use-mark-notification-as-read");

const mockPush = jest.fn();
const mockRefetch = jest.fn();
const mockMarkAsRead = jest.fn();

const notification: SignalLabNotification =
{
  id: "notification-1",
  type: "OrganizationInvitationReceived",
  targetType: "OrganizationInvitation",
  targetId: "invitation-1",
  createdAt: "2026-01-01T10:00:00Z",
  readAt: null,
};

function mockNotifications(
  items: SignalLabNotification[],
  overrides: Record<string, unknown> = {}
): void
{
  jest.mocked(useCurrentUserNotifications).mockReturnValue(
  {
    data: items,
    isPending: false,
    isError: false,
    refetch: mockRefetch,
    ...overrides,
  } as unknown as ReturnType<typeof useCurrentUserNotifications>);
}

function mockUnreadCount(count: number): void
{
  jest.mocked(useUnreadNotificationCount).mockReturnValue(
  {
    data: { count },
    isPending: false,
    isError: false,
  } as unknown as ReturnType<typeof useUnreadNotificationCount>);
}

describe("NotificationTrigger", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();

    jest.mocked(useRouter).mockReturnValue(
    {
      push: mockPush,
    } as unknown as ReturnType<typeof useRouter>);

    jest.mocked(usePathname).mockReturnValue("/home");

    mockNotifications([notification]);
    mockUnreadCount(1);

    jest.mocked(useMarkNotificationAsRead).mockReturnValue(
    {
      mutate: mockMarkAsRead,
      isPending: false,
    } as unknown as ReturnType<typeof useMarkNotificationAsRead>);
  });

  it("renders the permanent Notifications control", () =>
  {
    render(<NotificationTrigger />);

    expect(
      screen.getByRole("button", { name: "Notifications" })
    ).toBeInTheDocument();

    expect(useCurrentUserNotifications).toHaveBeenCalledWith(false);
    expect(useUnreadNotificationCount).toHaveBeenCalled();
  });

  it("opens the popover and shows the unread count", async () =>
  {
    render(<NotificationTrigger />);

    fireEvent.click(
      screen.getByRole("button", { name: "Notifications" })
    );

    expect(
      await screen.findByText("1 unread")
    ).toBeInTheDocument();

    expect(
      screen.getByText("Organization invitation")
    ).toBeInTheDocument();

    expect(useCurrentUserNotifications).toHaveBeenCalledWith(true);
  });

  it("shows an empty state when there are no notifications", async () =>
  {
    mockNotifications([]);
    mockUnreadCount(0);

    render(<NotificationTrigger />);

    fireEvent.click(
      screen.getByRole("button", { name: "Notifications" })
    );

    expect(
      await screen.findByText("No notifications yet.")
    ).toBeInTheDocument();

    expect(screen.queryByText("0 unread")).not.toBeInTheDocument();
  });

  it("navigates to the invitation and marks an unread notification", async () =>
  {
    render(<NotificationTrigger />);

    fireEvent.click(
      screen.getByRole("button", { name: "Notifications" })
    );

    fireEvent.click(
      await screen.findByRole("button", {
        name: /Organization invitation/i,
      })
    );

    expect(mockMarkAsRead).toHaveBeenCalledWith("notification-1");

    expect(mockPush).toHaveBeenCalledWith(
      "/organization-invitations/invitation-1?from=%2Fhome"
    );
  });

  it("does not mark an already read notification again", async () =>
  {
    mockNotifications(
    [
      {
        ...notification,
        readAt: "2026-01-02T10:00:00Z",
      },
    ]);

    mockUnreadCount(0);

    render(<NotificationTrigger />);

    fireEvent.click(
      screen.getByRole("button", { name: "Notifications" })
    );

    fireEvent.click(
      await screen.findByRole("button", {
        name: /Organization invitation/i,
      })
    );

    expect(mockMarkAsRead).not.toHaveBeenCalled();

    expect(mockPush).toHaveBeenCalledWith(
      "/organization-invitations/invitation-1?from=%2Fhome"
    );
  });

  it("allows retrying when notifications cannot be loaded", async () =>
  {
    mockNotifications([], { isError: true });

    render(<NotificationTrigger />);

    fireEvent.click(
      screen.getByRole("button", { name: "Notifications" })
    );

    expect(
      await screen.findByRole("alert")
    ).toHaveTextContent("Unable to load notifications.");

    fireEvent.click(
      screen.getByRole("button", { name: "Retry" })
    );

    await waitFor(() =>
    {
      expect(mockRefetch).toHaveBeenCalledTimes(1);
    });
  });

  it("navigates even if marking a notification as read throws", async () =>
  {
    mockMarkAsRead.mockImplementation(() =>
    {
      throw new Error("Unable to mark notification as read.");
    });

    render(<NotificationTrigger />);

    fireEvent.click(
      screen.getByRole("button", { name: "Notifications" })
    );

    fireEvent.click(
      await screen.findByRole("button", {
        name: /Organization invitation/i,
      })
    );

    expect(mockMarkAsRead).toHaveBeenCalledWith("notification-1");

    expect(mockPush).toHaveBeenCalledWith(
      "/organization-invitations/invitation-1?from=%2Fhome"
    );
  });

  it("preserves the current internal pathname as the return origin", async () =>
  {
    jest.mocked(usePathname).mockReturnValue(
      "/organizations/organization-1/projects"
    );

    render(<NotificationTrigger />);

    fireEvent.click(
      screen.getByRole("button", { name: "Notifications" })
    );

    fireEvent.click(
      await screen.findByRole("button", {
        name: /Organization invitation/i,
      })
    );

    expect(mockPush).toHaveBeenCalledWith(
      "/organization-invitations/invitation-1" +
      "?from=%2Forganizations%2Forganization-1%2Fprojects"
    );
  });

  it("does not preserve an unsafe return origin", async () =>
  {
    jest.mocked(usePathname).mockReturnValue(
      "//example.com/redirect"
    );

    render(<NotificationTrigger />);

    fireEvent.click(
      screen.getByRole("button", { name: "Notifications" })
    );

    fireEvent.click(
      await screen.findByRole("button", {
        name: /Organization invitation/i,
      })
    );

    expect(mockPush).toHaveBeenCalledWith(
      "/organization-invitations/invitation-1"
    );
  });

  it("allows opening notifications when the unread count fails", async () =>
  {
    jest.mocked(useUnreadNotificationCount).mockReturnValue(
    {
      data: undefined,
      isPending: false,
      isError: true,
    } as unknown as ReturnType<typeof useUnreadNotificationCount>);

    render(<NotificationTrigger />);

    fireEvent.click(
      screen.getByRole("button", { name: "Notifications" })
    );

    expect(
      await screen.findByText("Organization invitation")
    ).toBeInTheDocument();

    expect(
      screen.queryByText("1 unread")
    ).not.toBeInTheDocument();
  });
});