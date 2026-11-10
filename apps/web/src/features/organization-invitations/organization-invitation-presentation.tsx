"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";

import { Button } from "@/components/ui/button";
import { ApiError } from "@/lib/api/api-error";

import {
  HOME_ROUTE,
  buildOrganizationRoute,
} from "@/navigation/navigation-routes";

import { useOrganizationInvitation } from "./use-organization-invitation";
import { useCurrentUserInvitations } from "./use-current-user-invitations";
import { useAcceptOrganizationInvitation } from "./use-accept-organization-invitation";
import { useDeclineOrganizationInvitation } from "./use-decline-organization-invitation";

import type {
  OrganizationInvitationStatus,
} from "./organization-invitation";

import { useSearchParams } from "next/navigation";

import {
  getSafeInvitationOrigin,
} from "./invitation-navigation";

type OrganizationInvitationPresentationProps = Readonly<
{
  invitationId: string;
}>;

function getEffectiveStatus(
  status: OrganizationInvitationStatus,
  expiresAt: string
): OrganizationInvitationStatus
{
  if (
    status === "Pending" &&
    new Date(expiresAt).getTime() <= Date.now()
  )
  {
    return "Expired";
  }

  return status;
}

function getActionError(error: unknown): string
{
  if (error instanceof ApiError)
  {
    if (error.status === 403 || error.status === 404)
    {
      return "This invitation is not accessible with your current account.";
    }

    if (error.status === 409)
    {
      return "This invitation is no longer available for this action.";
    }
  }

  return "Unable to process this invitation. Please try again.";
}

export function OrganizationInvitationPresentation(
  { invitationId }: OrganizationInvitationPresentationProps
)
{
  const router = useRouter();

  const searchParams = useSearchParams();

  const returnRoute = getSafeInvitationOrigin(searchParams.get("from")) ?? HOME_ROUTE;

  function handleBack(): void
  {
    router.push(returnRoute);
  }

  const invitationQuery = useOrganizationInvitation(invitationId);
  const personalInvitationsQuery = useCurrentUserInvitations();

  const acceptInvitation = useAcceptOrganizationInvitation(invitationId);
  const declineInvitation = useDeclineOrganizationInvitation(invitationId);

  const [actionError, setActionError] = useState<string | null>(null);

  const invitation = invitationQuery.data;

  const personalInvitation = personalInvitationsQuery.data?.find((item) =>
  {
    return item.id === invitationId;
  });

  const organizationName =
    personalInvitation?.organizationName ?? null;

  const isMutating =
    acceptInvitation.isPending ||
    declineInvitation.isPending;

  async function handleAccept(): Promise<void>
  {
    if (invitation === undefined || isMutating)
    {
      return;
    }

    setActionError(null);

    try
    {
      await acceptInvitation.mutateAsync();

      router.replace(
        buildOrganizationRoute(invitation.organizationId, "all")
      );
    }
    catch (error)
    {
      setActionError(getActionError(error));
      void invitationQuery.refetch();
    }
  }

  async function handleDecline(): Promise<void>
  {
    if (invitation === undefined || isMutating)
    {
      return;
    }

    setActionError(null);

    try
    {
      await declineInvitation.mutateAsync();
    }
    catch (error)
    {
      setActionError(getActionError(error));
      void invitationQuery.refetch();
    }
  }

  if (invitationQuery.isPending)
  {
    return (
      <div className="mx-auto max-w-3xl px-6 py-10">
        <p role="status" className="text-sm text-muted-foreground">
          Loading invitation...
        </p>
      </div>
    );
  }

  if (invitationQuery.isError || invitation === undefined)
  {
    return (
      <div className="mx-auto max-w-3xl space-y-5 px-6 py-10">
        <h1 className="text-2xl font-semibold">
          Invitation unavailable
        </h1>

        <p role="alert" className="text-sm text-muted-foreground">
          This invitation could not be found or is not accessible
          with your current account.
        </p>

        <div className="flex gap-3">
          <Button
            variant="outline"
            onClick={() =>
            {
              void invitationQuery.refetch();
            }}
          >
            Retry
          </Button>

          <Button
            variant="outline"
            onClick={handleBack}
          >
            Back
          </Button>
        </div>
      </div>
    );
  }

  const status = getEffectiveStatus(
    invitation.status,
    invitation.expiresAt
  );

  return (
    <div className="mx-auto flex w-full max-w-3xl flex-col gap-6 px-6 py-10">
      <header className="space-y-2">
        <h1 className="text-2xl font-semibold tracking-tight">
          Organization invitation
        </h1>

        <p className="text-sm text-muted-foreground">
          Review your invitation before responding.
        </p>
      </header>

      <section className="space-y-5 rounded-xl border bg-card p-6">
        <div className="space-y-1">
          <p className="text-xs text-muted-foreground">
            Organization
          </p>

          <p className="text-lg font-semibold">
            {organizationName ?? "Organization"}
          </p>
        </div>

        <div className="grid gap-4 sm:grid-cols-2">
          <div>
            <p className="text-xs text-muted-foreground">
              Your role
            </p>

            <p className="mt-1 font-medium">
              {invitation.roleName}
            </p>
          </div>

          <div>
            <p className="text-xs text-muted-foreground">
              Status
            </p>

            <p className="mt-1 font-medium">
              {status}
            </p>
          </div>
        </div>

        {status === "Pending"
          ? (
            <>
              <div className="rounded-lg border p-4">
                <h2 className="font-medium">
                  Your information in this Organization
                </h2>

                <p className="mt-2 text-sm text-muted-foreground">
                  If you accept, your profile name, when provided,
                  and the email address associated with your SignalLab
                  account will be visible to members authorized to
                  access this Organization&apos;s administration.
                </p>
              </div>

              {actionError !== null
                ? (
                  <p role="alert" className="text-sm text-destructive">
                    {actionError}
                  </p>
                )
                : null}

              <div className="flex flex-wrap justify-end gap-3">
                <Button
                  type="button"
                  variant="outline"
                  disabled={isMutating}
                  onClick={() =>
                  {
                    void handleDecline();
                  }}
                >
                  {declineInvitation.isPending
                    ? "Declining..."
                    : "Decline"}
                </Button>

                <Button
                  type="button"
                  disabled={isMutating}
                  onClick={() =>
                  {
                    void handleAccept();
                  }}
                >
                  {acceptInvitation.isPending
                    ? "Accepting..."
                    : "Accept invitation"}
                </Button>
              </div>
            </>
          )
          : (
            <p className="text-sm text-muted-foreground">
              {status === "Accepted"
                ? "This invitation has already been accepted."
                : status === "Declined"
                  ? "You have declined this invitation."
                  : status === "Cancelled"
                    ? "This invitation has been cancelled."
                    : "This invitation has expired."}
            </p>
          )}
      </section>

      <div>
        <Button
          type="button"
          variant="outline"
          onClick={handleBack}
        >
          Back
        </Button>
      </div>
    </div>
  );
}