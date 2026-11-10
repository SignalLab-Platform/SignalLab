"use client";

import { useState, type FormEvent } from "react";
import { CheckCircle2, Copy } from "lucide-react";

import { Button } from "@/components/ui/button";

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

import { ApiError } from "@/lib/api/api-error";

import type {
  OrganizationRole,
} from "@/features/organizations/organization-role";

import {
  useCreateOrganizationInvitation,
} from "./use-create-organization-invitation";

type InvitationDuration = 1 | 7 | 30;

type CreateOrganizationInvitationDialogProps = Readonly<
{
  organizationId: string;
  roles: readonly OrganizationRole[];
  open: boolean;
  onOpenChange: (open: boolean) => void;
  canInviteOwner: boolean;
}>;

function getCreationErrorMessage(error: unknown): string
{
  if (error instanceof ApiError)
  {
    switch (error.code)
    {
      case "organization_invitation_already_pending":
        return "An invitation is already pending for this user. You can find it in the Invitations tab.";

      case "organization_already_member":
        return "This user is already an active member of the Organization.";

      case "organization_invitation_recipient_unavailable":
        return "Unable to invite this recipient. Check the address or contact the user.";

      case "organization_invitation_invalid_recipient_email":
        return "Enter a valid email address.";

      case "organization_invitation_invalid_role":
      case "organization_invitation_invalid_role_id":
        return "The selected role is no longer available.";

      case "organization_manage_members_denied":
        return "You no longer have permission to invite members.";

      case "organization_invitation_owner_denied":
        return "Only an Organization Owner can invite another Owner.";
    }
  }

  return "Unable to create the invitation. Please try again.";
}

export function CreateOrganizationInvitationDialog(
  {
    organizationId,
    roles,
    open,
    onOpenChange,
    canInviteOwner,
  }: CreateOrganizationInvitationDialogProps
)
{
  const [email, setEmail] = useState("");
  const [roleId, setRoleId] = useState("");
  const [duration, setDuration] = useState<InvitationDuration>(7);
  const [createdInvitationId, setCreatedInvitationId] =
    useState<string | null>(null);
  const [copyError, setCopyError] = useState(false);
  const [copied, setCopied] = useState(false);

  const createInvitation = useCreateOrganizationInvitation(organizationId);

  const availableRoles = roles.filter((role) =>
  {
    return !role.isOwner || canInviteOwner;
  });

  const canSubmit =
    email.trim().length > 0 &&
    availableRoles.some((role) => role.id === roleId) &&
    !createInvitation.isPending;

  const invitationLink = createdInvitationId === null
    ? null
    : `${typeof window === "undefined" ? "" : window.location.origin}/organization-invitations/${createdInvitationId}`;

  function closeDialog(): void
  {
    setEmail("");
    setRoleId("");
    setDuration(7);
    setCreatedInvitationId(null);
    setCopyError(false);
    setCopied(false);
    createInvitation.reset();
    onOpenChange(false);
  }

  function handleOpenChange(nextOpen: boolean): void
  {
    if (createInvitation.isPending)
    {
      return;
    }

    if (!nextOpen)
    {
      closeDialog();
      return;
    }

    onOpenChange(true);
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>): Promise<void>
  {
    event.preventDefault();

    if (!canSubmit)
    {
      return;
    }

    try
    {
      const invitation = await createInvitation.mutateAsync(
      {
        email: email.trim(),
        roleId,
        duration,
      });

      setCreatedInvitationId(invitation.id);
    }
    catch
    {
      // The mutation retains its error for display.
    }
  }

  async function handleCopyLink(): Promise<void>
  {
    if (invitationLink === null)
    {
      return;
    }

    try
    {
      await navigator.clipboard.writeText(invitationLink);
      setCopied(true);
      setCopyError(false);
    }
    catch
    {
      setCopied(false);
      setCopyError(true);
    }
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent
        className="sm:max-w-md"
        showCloseButton={!createInvitation.isPending}
      >
        {createdInvitationId === null
          ? (
            <>
              <DialogHeader>
                <DialogTitle>Invite member</DialogTitle>

                <DialogDescription>
                  Invite an existing SignalLab user to join this Organization.
                </DialogDescription>
              </DialogHeader>

              <form onSubmit={handleSubmit} className="grid gap-4">
                <div className="grid gap-2">
                  <Label htmlFor="invitation-email">
                    Email address
                  </Label>

                  <Input
                    id="invitation-email"
                    type="email"
                    autoComplete="off"
                    placeholder="user@example.com"
                    value={email}
                    onChange={(event) =>
                    {
                      setEmail(event.target.value);
                    }}
                    disabled={createInvitation.isPending}
                    required
                  />

                  <p className="text-xs text-muted-foreground">
                    Enter the complete email address of an existing SignalLab account.
                  </p>
                </div>

                <div className="grid gap-2">
                  <Label htmlFor="invitation-role">
                    Organization role
                  </Label>

                  <Select
                    value={roleId}
                    onValueChange={setRoleId}
                    disabled={createInvitation.isPending}
                  >
                    <SelectTrigger id="invitation-role" className="w-full">
                      <SelectValue placeholder="Select a role" />
                    </SelectTrigger>

                    <SelectContent>
                      {availableRoles.map((role) =>
                      {
                        return (
                          <SelectItem key={role.id} value={role.id}>
                            {role.name}
                          </SelectItem>
                        );
                      })}
                    </SelectContent>
                  </Select>
                </div>

                <div className="grid gap-2">
                  <Label htmlFor="invitation-duration">
                    Invitation expires after
                  </Label>

                  <Select
                    value={String(duration)}
                    onValueChange={(value) =>
                    {
                      setDuration(Number(value) as InvitationDuration);
                    }}
                    disabled={createInvitation.isPending}
                  >
                    <SelectTrigger id="invitation-duration" className="w-full">
                      <SelectValue />
                    </SelectTrigger>

                    <SelectContent>
                      <SelectItem value="1">24 hours</SelectItem>
                      <SelectItem value="7">7 days</SelectItem>
                      <SelectItem value="30">30 days</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                {createInvitation.isError
                  ? (
                    <p role="alert" className="text-sm text-destructive">
                      {getCreationErrorMessage(createInvitation.error)}
                    </p>
                  )
                  : null}

                <DialogFooter>
                  <Button
                    type="button"
                    variant="outline"
                    onClick={closeDialog}
                    disabled={createInvitation.isPending}
                  >
                    Cancel
                  </Button>

                  <Button type="submit" disabled={!canSubmit}>
                    {createInvitation.isPending
                      ? "Sending..."
                      : "Send invitation"}
                  </Button>
                </DialogFooter>
              </form>
            </>
          )
          : (
            <>
              <DialogHeader>
                <div className="mb-2">
                  <CheckCircle2
                    aria-hidden="true"
                    className="size-8 text-primary"
                  />
                </div>

                <DialogTitle>Invitation sent</DialogTitle>

                <DialogDescription>
                  An invitation has been created for {email.trim()}.
                </DialogDescription>
              </DialogHeader>

              <div className="grid gap-2">
                <Label htmlFor="created-invitation-link">
                  Invitation link
                </Label>

                <div className="flex gap-2">
                  <Input
                    id="created-invitation-link"
                    value={invitationLink ?? ""}
                    readOnly
                    onFocus={(event) =>
                    {
                      event.target.select();
                    }}
                  />

                  <Button
                    type="button"
                    variant="outline"
                    aria-label="Copy invitation link"
                    onClick={() =>
                    {
                      void handleCopyLink();
                    }}
                  >
                    <Copy aria-hidden="true" className="size-4" />
                  </Button>
                </div>

                {copied
                  ? (
                    <p role="status" className="text-xs text-muted-foreground">
                      Link copied to clipboard.
                    </p>
                  )
                  : null}

                {copyError
                  ? (
                    <p role="alert" className="text-xs text-destructive">
                      Unable to copy automatically. Select and copy the link manually.
                    </p>
                  )
                  : null}

                <p className="text-xs text-muted-foreground">
                  The recipient can also find this invitation in their notifications.
                </p>
              </div>

              <DialogFooter>
                <Button type="button" onClick={closeDialog}>
                  Done
                </Button>
              </DialogFooter>
            </>
          )}
      </DialogContent>
    </Dialog>
  );
}