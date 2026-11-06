"use client";

import { Building2, Plus } from "lucide-react";
import Link from "next/link";
import { useState } from "react";

import { useOrganizations } from "@/features/organizations/use-organizations";
import { buildOrganizationRoute } from "@/navigation/navigation-routes";
import { CreateOrganizationDialog } from "@/features/organizations/create-organization-dialog";

export function OrganizationsPersonalExplorer()
{
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const organizationsQuery = useOrganizations();

  return (
    <section aria-labelledby="organizations-heading" className="space-y-4">
      <header>
        <h2
          id="organizations-heading"
          className="text-lg font-semibold tracking-tight"
        >
          Organizations
        </h2>

        <p className="mt-1 text-sm text-muted-foreground">
          Your organizations and workspaces.
        </p>
      </header>

      {organizationsQuery.isError
        ? (
          <div
            role="alert"
            className="rounded-xl border border-destructive/40 bg-card p-5"
          >
            <p className="font-medium">
              We couldn&apos;t load your organizations.
            </p>

            <p className="mt-1 text-sm text-muted-foreground">
              Refresh the page to try again.
            </p>
          </div>
        )
        : (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            <button
              type="button"
              onClick={() =>
              {
                setCreateDialogOpen(true);
              }}
              className="flex min-h-36 flex-col items-start gap-3 rounded-xl border border-dashed border-border bg-card p-5 text-left transition-colors hover:bg-accent focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
            >
              <span className="flex size-10 items-center justify-center rounded-lg bg-muted">
                <Plus aria-hidden="true" className="size-5" />
              </span>

              <span className="font-medium">
                Create Organization
              </span>
            </button>

            {organizationsQuery.isPending
              ? (
                <>
                  <div className="min-h-36 animate-pulse rounded-xl border bg-card" />
                  <div className="min-h-36 animate-pulse rounded-xl border bg-card" />
                </>
              )
              : organizationsQuery.data?.map((organization) =>
                {
                  return (
                    <Link
                      key={organization.id}
                      href={buildOrganizationRoute(organization.id, "all")}
                      scroll={false}
                      className="flex min-h-36 flex-col items-start gap-3 rounded-xl border bg-card p-5 transition-colors hover:bg-accent focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                    >
                      <span className="flex size-10 items-center justify-center rounded-lg bg-muted">
                        <Building2 aria-hidden="true" className="size-5" />
                      </span>

                      <span className="min-w-0">
                        <span className="block truncate font-medium">
                          {organization.name}
                        </span>

                        <span className="mt-1 block text-sm text-muted-foreground">
                          {organization.country}
                        </span>
                      </span>
                    </Link>
                  );
                })}
          </div>
        )}

      <CreateOrganizationDialog
        open={createDialogOpen}
        onOpenChange={setCreateDialogOpen}
      />
    </section>
  );
}