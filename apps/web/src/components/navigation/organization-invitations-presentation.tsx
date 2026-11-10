"use client";

import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";

import { useState } from "react";
import { UserPlus, Copy, XCircle } from "lucide-react";

import {
  AlertDialog,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";

import { ApiError } from "@/lib/api/api-error";

import {
  useCancelOrganizationInvitation,
} from "@/features/organization-invitations/use-cancel-organization-invitation";

import type {
  OrganizationInvitation,
  OrganizationInvitationStatus,
} from "@/features/organization-invitations/organization-invitation";

type OrganizationInvitationsContentProps = Readonly<
{
  invitations: readonly OrganizationInvitation[];
  isPending: boolean;
  isError: boolean;
  onRetry: () => void;
  organizationId: string;
  canManageInvitations: boolean;
}>;

function formatInvitationDate(value: string): string
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

function getInvitationStatus(
  invitation: OrganizationInvitation
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

function getRecipientName(invitation: OrganizationInvitation): string | null
{
  const name = [
    invitation.invitedUserFirstName?.trim(),
    invitation.invitedUserLastName?.trim(),
  ]
    .filter(Boolean)
    .join(" ");

  return name.length > 0 ? name : null;
}

type OrganizationInvitationsActionsProps = Readonly<
{
  canInvite: boolean;
  onInvite: () => void;
}>;

export function OrganizationInvitationsActions(
  { canInvite, onInvite }: OrganizationInvitationsActionsProps
)
{
  if (!canInvite)
  {
    return null;
  }

  return (
    <Button type="button" size="sm" onClick={onInvite}>
      <UserPlus aria-hidden="true" className="size-4" />
      Invite member
    </Button>
  );
}

export function OrganizationInvitationsContent(
  {
    invitations,
    isPending,
    isError,
    onRetry,
    organizationId,
    canManageInvitations,
  }: OrganizationInvitationsContentProps
)
{
  const cancelInvitation = useCancelOrganizationInvitation(organizationId);

  const [invitationToCancel, setInvitationToCancel] = useState<OrganizationInvitation | null>(null);

  const [actionError, setActionError] = useState<string | null>(null);
  const [copiedInvitationId, setCopiedInvitationId] = useState<string | null>(null);

  function getInvitationLink(invitationId: string): string
  {
    return new URL(
      `/organization-invitations/${invitationId}`,
      window.location.origin
    ).toString();
  }

  async function handleCopyLink(invitationId: string): Promise<void>
  {
    setActionError(null);
    setCopiedInvitationId(null);

    try
    {
      await navigator.clipboard.writeText(getInvitationLink(invitationId));
      setCopiedInvitationId(invitationId);
    }
    catch
    {
      setActionError(
        "Automatic copying failed. Select the invitation link to copy it manually."
      );
    }
  }

  async function handleCancelInvitation(): Promise<void>
  {
    if (invitationToCancel === null)
    {
      return;
    }

    setActionError(null);

    try
    {
      await cancelInvitation.mutateAsync(invitationToCancel.id);
      setInvitationToCancel(null);
    }
    catch (error)
    {
      if (error instanceof ApiError && error.status === 403)
      {
        setActionError("You no longer have permission to cancel this invitation.");
      }
      else if (error instanceof ApiError && error.status === 409)
      {
        setActionError(
          "This invitation can no longer be cancelled. Refresh the list to see its current status."
        );
      }
      else
      {
        setActionError("Unable to cancel the invitation. Please try again.");
      }
    }
  }

  if (isPending)
  {
    return (
      <div
        role="status"
        className="rounded-xl border bg-card p-8 text-sm text-muted-foreground"
      >
        Loading organization invitations...
      </div>
    );
  }

  if (isError)
  {
    return (
      <div role="alert" className="rounded-xl border bg-card p-8">
        <p className="font-medium">
          Unable to load organization invitations.
        </p>

        <p className="mt-2 text-sm text-muted-foreground">
          Please try again.
        </p>

        <Button
          type="button"
          variant="outline"
          className="mt-4"
          onClick={onRetry}
        >
          Retry
        </Button>
      </div>
    );
  }

  return (
    <>
    {actionError !== null && invitationToCancel === null
      ? (
        <p role="alert" className="mb-3 text-sm text-destructive">
          {actionError}
        </p>
      )
      : null}

    <div className="overflow-hidden rounded-xl border bg-card">
      <div className="border-b px-6 py-4">
        <p className="text-sm text-muted-foreground">
          {invitations.length} invitations
        </p>
      </div>

      {invitations.length === 0
        ? (
          <div className="px-6 py-12 text-center">
            <p className="font-medium">
              No invitations
            </p>

            <p className="mt-2 text-sm text-muted-foreground">
              Invitations sent to existing SignalLab users
              will appear here.
            </p>
          </div>
        )
        : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead className="border-b bg-muted/30 text-xs text-muted-foreground">
                <tr>
                  <th scope="col" className="px-6 py-3 font-medium">
                    Recipient
                  </th>

                  <th scope="col" className="px-6 py-3 font-medium">
                    Role
                  </th>

                  <th scope="col" className="px-6 py-3 font-medium">
                    Status
                  </th>

                  <th scope="col" className="px-6 py-3 font-medium">
                    Invited at
                  </th>

                  <th scope="col" className="px-6 py-3 font-medium">
                    Expires at
                  </th>

                  <th
                    scope="col"
                    className="w-16 px-6 py-3 text-right font-medium"
                  >
                    <span className="sr-only">
                      Actions
                    </span>
                  </th>
                </tr>
              </thead>

              <tbody className="divide-y">
                {invitations.map((invitation) =>
                {
                  const status = getInvitationStatus(invitation);
                  const recipientName = getRecipientName(invitation);

                  return (
                    <tr key={invitation.id}>
                      <td className="px-6 py-4">
                        {recipientName !== null
                          ? (
                            <div className="font-medium">
                              {recipientName}
                            </div>
                          )
                          : null}

                        <div className="text-sm text-muted-foreground">
                          {invitation.invitedUserEmail}
                        </div>
                      </td>

                      <td className="px-6 py-4">
                        {invitation.roleName}
                      </td>

                      <td className="px-6 py-4">
                        <span
                          className={
                            status === "Pending"
                              ? "font-medium text-foreground"
                              : "text-muted-foreground"
                          }
                        >
                          {status}
                        </span>
                      </td>

                      <td className="px-6 py-4 text-muted-foreground">
                        {formatInvitationDate(invitation.createdAt)}
                      </td>

                      <td className="px-6 py-4 text-muted-foreground">
                        {formatInvitationDate(invitation.expiresAt)}
                      </td>

                      <td className="px-6 py-4">
                        {canManageInvitations
                          ? (
                            <div className="flex min-w-64 items-center justify-end gap-2">
                              <Input
                                aria-label={`Invitation link for ${invitation.invitedUserEmail}`}
                                value={getInvitationLink(invitation.id)}
                                readOnly
                                onFocus={(event) =>
                                {
                                  event.target.select();
                                }}
                                className="min-w-0 flex-1 text-xs"
                              />

                              <Button
                                type="button"
                                variant="outline"
                                size="sm"
                                aria-label={`Copy invitation link for ${invitation.invitedUserEmail}`}
                                onClick={() =>
                                {
                                  void handleCopyLink(invitation.id);
                                }}
                              >
                                <Copy aria-hidden="true" className="size-4" />
                                Copy
                              </Button>

                              {status === "Pending"
                                ? (
                                  <Button
                                    type="button"
                                    variant="ghost"
                                    size="sm"
                                    aria-label={`Cancel invitation for ${invitation.invitedUserEmail}`}
                                    onClick={() =>
                                    {
                                      setActionError(null);
                                      setInvitationToCancel(invitation);
                                    }}
                                  >
                                    <XCircle aria-hidden="true" className="size-4" />
                                    Cancel
                                  </Button>
                                )
                                : null}

                              {copiedInvitationId === invitation.id
                                ? (
                                  <span role="status" className="text-xs text-muted-foreground">
                                    Copied
                                  </span>
                                )
                                : null}
                            </div>
                          )
                          : null}
                        </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}

      <AlertDialog
        open={invitationToCancel !== null}
        onOpenChange={(open) =>
        {
          if (!open && !cancelInvitation.isPending)
          {
            setInvitationToCancel(null);
            setActionError(null);
          }
        }}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              Cancel invitation?
            </AlertDialogTitle>

            <AlertDialogDescription>
              The invitation sent to{" "}
              <strong>{invitationToCancel?.invitedUserEmail}</strong>{" "}
              will no longer be valid. The recipient will not be
              able to accept it afterwards.
            </AlertDialogDescription>
          </AlertDialogHeader>

          {actionError !== null
            ? (
              <p role="alert" className="text-sm text-destructive">
                {actionError}
              </p>
            )
            : null}

          <AlertDialogFooter>
            <AlertDialogCancel disabled={cancelInvitation.isPending}>
              Keep invitation
            </AlertDialogCancel>

            <Button
              type="button"
              variant="destructive"
              disabled={cancelInvitation.isPending}
              onClick={() =>
              {
                void handleCancelInvitation();
              }}
            >
              {cancelInvitation.isPending
                ? "Cancelling..."
                : "Cancel invitation"}
            </Button>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
    </>
  );
}