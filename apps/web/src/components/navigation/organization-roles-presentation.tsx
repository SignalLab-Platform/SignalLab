"use client";

import { ChevronDown, ShieldCheck } from "lucide-react";

import { Button } from "@/components/ui/button";

import type
{
  OrganizationRole,
} from "@/features/organizations/organization-role";

type OrganizationRolesContentProps =
{
  roles: OrganizationRole[];
  isPending: boolean;
  isError: boolean;
  onRetry: () => void;
};

function formatPermission(permission: string)
{
  return permission
    .replace(/([a-z])([A-Z])/g, "$1 $2")
    .replace(/([A-Z])([A-Z][a-z])/g, "$1 $2");
}

function OrganizationRoleItem(
  { role }: Readonly<{ role: OrganizationRole }>
)
{
  return (
    <details className="group border-b border-border last:border-b-0">
      <summary className="flex cursor-pointer list-none items-center justify-between gap-4 py-5 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring [&::-webkit-details-marker]:hidden">
        <div className="min-w-0 space-y-1">
          <div className="flex flex-wrap items-center gap-2">
            <span className="text-sm font-medium">
              {role.name}
            </span>

            {role.isOwner
              ? (
                <span className="inline-flex items-center gap-1 rounded-md bg-muted px-2 py-0.5 text-xs font-medium">
                  <ShieldCheck aria-hidden="true" className="size-3" />
                  Owner
                </span>
              )
              : null}
          </div>

          {role.description
            ? (
              <p className="text-sm text-muted-foreground">
                {role.description}
              </p>
            )
            : null}
        </div>

        <ChevronDown
          aria-hidden="true"
          className="size-4 shrink-0 text-muted-foreground transition-transform group-open:rotate-180"
        />
      </summary>

      <div className="pb-5">
        <h3 className="mb-3 text-xs font-medium text-muted-foreground">
          Permissions
        </h3>

        {role.permissions.length > 0
          ? (
            <ul className="flex flex-wrap gap-2">
              {role.permissions.map((permission) =>
              {
                return (
                  <li
                    key={permission}
                    className="rounded-md border border-border bg-muted/40 px-2.5 py-1 text-xs"
                    title={permission}
                  >
                    {formatPermission(permission)}
                  </li>
                );
              })}
            </ul>
          )
          : (
            <p className="text-sm text-muted-foreground">
              No permissions assigned.
            </p>
          )}
      </div>
    </details>
  );
}

export function OrganizationRolesContent(
  {
    roles,
    isPending,
    isError,
    onRetry,
  }: Readonly<OrganizationRolesContentProps>
)
{
  return (
    <section
      aria-labelledby="organization-roles-title"
      className="mt-8 border-t border-border pt-6"
    >
      <div className="space-y-1">
        <h2
          id="organization-roles-title"
          className="text-base font-semibold"
        >
          Roles & Permissions
        </h2>

        <p className="text-sm text-muted-foreground">
          View the roles available in this organization and their permissions.
        </p>
      </div>

      <div className="mt-5">
        {isPending
          ? (
            <p role="status" className="text-sm text-muted-foreground">
              Loading organization roles...
            </p>
          )
          : isError
            ? (
              <div role="alert" className="space-y-3">
                <p className="text-sm text-destructive">
                  Unable to load organization roles.
                </p>

                <Button
                  type="button"
                  variant="outline"
                  onClick={onRetry}
                >
                  Retry roles
                </Button>
              </div>
            )
            : roles.length === 0
              ? (
                <p role="status" className="text-sm text-muted-foreground">
                  No roles found for this organization.
                </p>
              )
              : (
                <div className="divide-y divide-border">
                  {roles.map((role) =>
                  {
                    return (
                      <OrganizationRoleItem
                        key={role.id}
                        role={role}
                      />
                    );
                  })}
                </div>
              )}
      </div>
    </section>
  );
}