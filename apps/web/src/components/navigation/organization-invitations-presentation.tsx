"use client";

import { MailPlus } from "lucide-react";

import { Button } from "@/components/ui/button";

export function OrganizationInvitationsActions()
{
  return (
    <Button
      type="button"
      size="sm"
      disabled
      title="Invitations will be available in SL-031."
    >
      <MailPlus aria-hidden="true" className="size-4" />
      Invite member
    </Button>
  );
}

export function OrganizationInvitationsContent()
{
  return (
    <div className="overflow-hidden rounded-xl border bg-card">
      <div
        role="status"
        className="border-b px-6 py-4 text-sm text-muted-foreground"
      >
        Invitation data is not available yet.
        Invitation management will be introduced in SL-031.
      </div>

      <div className="overflow-x-auto">
        <table className="w-full text-left text-sm">
          <thead className="border-b bg-muted/30 text-xs text-muted-foreground">
            <tr>
              <th scope="col" className="px-6 py-3 font-medium">
                Email
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
        </table>
      </div>

      <div className="px-6 py-12 text-center">
        <p className="font-medium">
          Invitations are not available yet
        </p>

        <p className="mt-2 text-sm text-muted-foreground">
          Invitation history and management will be available
          when OrganizationInvitation is implemented.
        </p>
      </div>
    </div>
  );
}