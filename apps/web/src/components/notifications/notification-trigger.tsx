"use client";

import { Bell, Mail } from "lucide-react";
import { usePathname, useRouter } from "next/navigation";
import { useState } from "react";

import { Button } from "@/components/ui/button";

import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";

import {
  useCurrentUserNotifications,
} from "@/features/notifications/use-current-user-notifications";

import {
  useUnreadNotificationCount,
} from "@/features/notifications/use-unread-notification-count";

import {
  useMarkNotificationAsRead,
} from "@/features/notifications/use-mark-notification-as-read";

import {
  buildOrganizationInvitationRoute,
  getSafeInvitationOrigin,
} from "@/features/organization-invitations/invitation-navigation";

import type {
  SignalLabNotification,
} from "@/features/notifications/notification";

function formatNotificationDate(value: string): string
{
  const date = new Date(value);

  if (Number.isNaN(date.getTime()))
  {
    return "Unknown date";
  }

  return new Intl.DateTimeFormat("en-US",
  {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

function getNotificationTitle(notification: SignalLabNotification): string
{
  switch (notification.type)
  {
    case "OrganizationInvitationReceived":
    {
      return "Organization invitation";
    }
  }
}

export function NotificationTrigger()
{
  const router = useRouter();
  const pathname = usePathname();

  const [open, setOpen] = useState(false);

  const notificationsQuery = useCurrentUserNotifications(open);
  const unreadCountQuery = useUnreadNotificationCount();
  const markAsRead = useMarkNotificationAsRead();

  const notifications = notificationsQuery.data ?? [];

  const unreadCount = unreadCountQuery.data?.count ?? 0;
  const hasUnreadNotifications = unreadCount > 0;

  function handleNotificationClick(
    notification: SignalLabNotification
  ): void
  {
    if (notification.targetType !== "OrganizationInvitation")
    {
      return;
    }

    const originPath = getSafeInvitationOrigin(pathname);

    const destination = buildOrganizationInvitationRoute(
      notification.targetId,
      originPath
    );

    setOpen(false);

    if (notification.readAt === null)
    {
      try
      {
        markAsRead.mutate(notification.id);
      }
      catch
      {
        router.push(destination);
        return;
      }
    }

    router.push(destination);
  }

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <button
          aria-label="Notifications"
          aria-haspopup="dialog"
          aria-expanded={open}
          className="relative inline-flex size-9 items-center justify-center rounded-md text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
          title="Notifications"
          type="button"
        >
          <Bell
            aria-hidden="true"
            className="size-4"
          />

          {hasUnreadNotifications
            ? (
              <span
                aria-hidden="true"
                className="absolute right-1 top-1 size-2 rounded-full bg-primary"
              />
            )
            : null}
        </button>
      </PopoverTrigger>

      <PopoverContent
        align="end"
        sideOffset={8}
        className="w-88 max-w-[calc(100vw-2rem)] p-0"
      >
        <div className="flex items-center justify-between gap-3 border-b px-4 py-3">
          <h2 className="text-sm font-semibold">
            Notifications
          </h2>

          {hasUnreadNotifications
            ? (
              <span className="text-xs text-muted-foreground">
                {unreadCount} unread
              </span>
            )
            : null}
        </div>

        {notificationsQuery.isPending
          ? (
            <div
              role="status"
              className="px-4 py-8 text-center text-sm text-muted-foreground"
            >
              Loading notifications...
            </div>
          )
          : notificationsQuery.isError
            ? (
              <div className="space-y-3 px-4 py-6">
                <p role="alert" className="text-sm text-destructive">
                  Unable to load notifications.
                </p>

                <Button
                  type="button"
                  size="sm"
                  variant="outline"
                  onClick={() =>
                  {
                    void notificationsQuery.refetch();
                  }}
                >
                  Retry
                </Button>
              </div>
            )
            : notifications.length === 0
              ? (
                <div className="px-4 py-8 text-center text-sm text-muted-foreground">
                  No notifications yet.
                </div>
              )
              : (
                <div className="max-h-96 overflow-y-auto">
                  <ul className="divide-y">
                    {notifications.map((notification) =>
                    {
                      const isUnread = notification.readAt === null;

                      return (
                        <li key={notification.id}>
                          <button
                            type="button"
                            className="flex w-full items-start gap-3 px-4 py-3 text-left transition-colors hover:bg-accent focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-ring"
                            onClick={() =>
                            {
                              handleNotificationClick(notification);
                            }}
                          >
                            <Mail
                              aria-hidden="true"
                              className="mt-0.5 size-4 shrink-0 text-muted-foreground"
                            />

                            <span className="min-w-0 flex-1">
                              <span className="block text-sm font-medium">
                                {getNotificationTitle(notification)}
                              </span>

                              <span className="mt-1 block text-xs text-muted-foreground">
                                You have received an Organization invitation.
                              </span>

                              <span className="mt-2 block text-xs text-muted-foreground">
                                {formatNotificationDate(notification.createdAt)}
                              </span>
                            </span>

                            {isUnread
                              ? (
                                <span
                                  aria-label="Unread"
                                  className="mt-1.5 size-2 shrink-0 rounded-full bg-primary"
                                />
                              )
                              : null}
                          </button>
                        </li>
                      );
                    })}
                  </ul>
                </div>
              )}

        <div className="border-t px-4 py-2">
          <p className="text-xs text-muted-foreground">
            Showing the 20 most recent notifications.
          </p>
        </div>
      </PopoverContent>
    </Popover>
  );
}