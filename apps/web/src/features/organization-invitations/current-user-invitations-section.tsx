"use client";

import Link from "next/link";
import { useId, useState } from "react";
import { ChevronDown, Mail } from "lucide-react";

import { Button } from "@/components/ui/button";

import {
  useCurrentUserInvitations,
} from "./use-current-user-invitations";

import {
  buildOrganizationInvitationRoute,
} from "./invitation-navigation";

import type {
  CurrentUserInvitation,
  OrganizationInvitationStatus,
} from "./organization-invitation";

function getEffectiveStatus(
  invitation: CurrentUserInvitation
): OrganizationInvitationStatus
{
  if (
    invitation.status === "Pending" &&
    new Date(invitation.expiresAt).getTime() <= Date.now()
  )
  {
    return "Expired";
  }

  return invitation.status;
}

function formatDate(value: string): string
{
  const date = new Date(value);

  if (Number.isNaN(date.getTime()))
  {
    return "Unknown";
  }

  return new Intl.DateTimeFormat("en-US",
  {
    year: "numeric",
    month: "short",
    day: "numeric",
  }).format(date);
}

export function CurrentUserInvitationsSection()
{
  const invitationsQuery = useCurrentUserInvitations();

  const contentId = useId();

  const [expandedOverride, setExpandedOverride] =
    useState<boolean | null>(null);

  const invitations = invitationsQuery.data ?? [];

  const pendingCount = invitations.filter((invitation) =>
  {
    return getEffectiveStatus(invitation) === "Pending";
  }).length;

  const isExpanded = expandedOverride ?? pendingCount > 0;

  return (
    <section aria-labelledby="personal-invitations-heading">
      <div className="flex items-center justify-between gap-4">
        <div className="flex items-center gap-3">
          <h2
            id="personal-invitations-heading"
            className="text-lg font-semibold"
          >
            Invitations
          </h2>

          {pendingCount > 0
            ? (
              <span className="rounded-full bg-muted px-2 py-0.5 text-xs font-medium">
                {pendingCount} pending
              </span>
            )
            : null}
        </div>

        <Button
          type="button"
          variant="ghost"
          size="sm"
          aria-expanded={isExpanded}
          aria-controls={contentId}
          aria-label={
            isExpanded
              ? "Collapse invitations"
              : "Expand invitations"
          }
          onClick={() =>
          {
            setExpandedOverride(!isExpanded);
          }}
        >
          <ChevronDown
            aria-hidden="true"
            className={
              isExpanded
                ? "size-4 rotate-180 transition-transform"
                : "size-4 transition-transform"
            }
          />
        </Button>
      </div>

      <div id={contentId} hidden={!isExpanded}>
        {isExpanded
          ? (
            <div className="mt-4">
              {invitationsQuery.isPending
                ? (
                  <p role="status" className="text-sm text-muted-foreground">
                    Loading invitations...
                  </p>
                )
                : invitationsQuery.isError
                  ? (
                    <div className="space-y-3">
                      <p role="alert" className="text-sm text-destructive">
                        Unable to load your invitations.
                      </p>

                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={() =>
                        {
                          void invitationsQuery.refetch();
                        }}
                      >
                        Retry
                      </Button>
                    </div>
                  )
                  : invitations.length === 0
                    ? (
                      <p className="rounded-xl border p-6 text-sm text-muted-foreground">
                        You have no invitations.
                      </p>
                    )
                    : (
                      <div className="overflow-hidden rounded-xl border">
                        <ul className="divide-y">
                          {invitations.map((invitation) =>
                          {
                            const status = getEffectiveStatus(invitation);

                            return (
                              <li key={invitation.id}>
                                <Link
                                  href={buildOrganizationInvitationRoute(
                                    invitation.id,
                                    "/home"
                                  )}
                                  className="flex items-center gap-4 px-5 py-4 transition-colors hover:bg-muted/40 focus-visible:outline-2 focus-visible:outline-offset-2px"
                                >
                                  <Mail
                                    aria-hidden="true"
                                    className="size-4 shrink-0 text-muted-foreground"
                                  />

                                  <div className="min-w-0 flex-1">
                                    <p className="truncate font-medium">
                                      {invitation.organizationName}
                                    </p>

                                    <p className="mt-1 text-sm text-muted-foreground">
                                      Role: {invitation.roleName}
                                    </p>
                                  </div>

                                  <div className="shrink-0 text-right">
                                    <p
                                      className={
                                        status === "Pending"
                                          ? "text-sm font-medium"
                                          : "text-sm text-muted-foreground"
                                      }
                                    >
                                      {status}
                                    </p>

                                    <p className="mt-1 text-xs text-muted-foreground">
                                      {formatDate(invitation.createdAt)}
                                    </p>
                                  </div>
                                </Link>
                              </li>
                            );
                          })}
                        </ul>
                      </div>
                    )}
            </div>
          )
          : null}
      </div>
    </section>
  );
}