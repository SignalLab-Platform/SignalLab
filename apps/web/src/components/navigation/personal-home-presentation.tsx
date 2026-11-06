"use client";

import
{
  ClipboardList,
  UserRound,
} from "lucide-react";

import { useCurrentUser } from "@/features/current-user/use-current-user";

import { OrganizationsPersonalExplorer } from "@/features/organizations/organizations-personal-explorer";

export function PersonalHomePresentation()
{
  const currentUserQuery = useCurrentUser();

  if (currentUserQuery.isPending)
  {
    return (
      <div className="mx-auto flex w-full max-w-5xl flex-col gap-6 px-6 py-8">
        <div className="space-y-2">
          <div className="h-8 w-48 animate-pulse rounded-md bg-muted" />
          <div className="h-5 w-72 animate-pulse rounded-md bg-muted" />
        </div>

        <div className="h-28 animate-pulse rounded-xl border bg-card" />

        <div className="space-y-8">
          <div className="space-y-4">
            <div className="h-6 w-40 animate-pulse rounded-md bg-muted" />

            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              <div className="h-36 animate-pulse rounded-xl border bg-card" />
              <div className="h-36 animate-pulse rounded-xl border bg-card" />
              <div className="h-36 animate-pulse rounded-xl border bg-card" />
            </div>
          </div>

          <div className="space-y-4">
            <div className="h-6 w-40 animate-pulse rounded-md bg-muted" />
            <div className="h-20 animate-pulse rounded-xl border bg-card" />
          </div>
        </div>
      </div>
    );
  }

  if (currentUserQuery.isError)
  {
    return (
      <div className="mx-auto flex w-full max-w-5xl flex-col gap-3 px-6 py-8">
        <h1 className="text-2xl font-semibold tracking-tight">
          Personal Home
        </h1>

        <div className="rounded-xl border border-destructive/40 bg-card p-5">
          <p className="font-medium">
            We couldn&apos;t load your SignalLab profile.
          </p>

          <p className="mt-1 text-sm text-muted-foreground">
            Refresh the page to try again.
          </p>
        </div>
      </div>
    );
  }

  const currentUser = currentUserQuery.data;

  return (
    <div className="mx-auto flex w-full max-w-5xl flex-col gap-6 px-6 py-8">
      <header>
        <h1 className="text-2xl font-semibold tracking-tight">
          Personal Home
        </h1>

        <p className="mt-1 text-sm text-muted-foreground">
          Your personal SignalLab workspace.
        </p>
      </header>

      <section
        aria-labelledby="profile-heading"
        className="rounded-xl border bg-card p-5"
      >
        <div className="flex items-start gap-4">
          <div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-muted">
            <UserRound
              aria-hidden="true"
              className="size-5"
            />
          </div>

          <div className="min-w-0">
            <h2
              id="profile-heading"
              className="font-medium"
            >
              Your profile
            </h2>

            <p className="mt-1 truncate text-sm text-muted-foreground">
              {currentUser.email}
            </p>
          </div>
        </div>
      </section>

      <div className="space-y-8">
        <OrganizationsPersonalExplorer />

        <section
          aria-labelledby="participations-heading"
          className="space-y-4"
        >
          <header>
            <h2
              id="participations-heading"
              className="text-lg font-semibold tracking-tight"
            >
              Participations
            </h2>

            <p className="mt-1 text-sm text-muted-foreground">
              Your ongoing study participations.
            </p>
          </header>

          <div className="rounded-xl border bg-card p-5">
            <div className="flex items-center gap-3">
              <ClipboardList
                aria-hidden="true"
                className="size-5 text-muted-foreground"
              />

              <p className="text-sm text-muted-foreground">
                No participations available yet.
              </p>
            </div>
          </div>
        </section>
      </div>
    </div>
  );
}
