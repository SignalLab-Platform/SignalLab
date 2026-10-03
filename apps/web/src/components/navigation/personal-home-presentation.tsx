"use client";

import
{
  Building2,
  ClipboardList,
  UserRound,
} from "lucide-react";

import { useCurrentUser } from "@/features/current-user/use-current-user";

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

        <div className="grid gap-4 lg:grid-cols-2">
          <div className="h-40 animate-pulse rounded-xl border bg-card" />
          <div className="h-40 animate-pulse rounded-xl border bg-card" />
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

      <div className="grid gap-4 lg:grid-cols-2">
        <section
          aria-labelledby="organizations-heading"
          className="rounded-xl border bg-card p-5"
        >
          <div className="flex items-start gap-4">
            <div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-muted">
              <Building2
                aria-hidden="true"
                className="size-5"
              />
            </div>

            <div>
              <h2
                id="organizations-heading"
                className="font-medium"
              >
                Organizations
              </h2>

              <p className="mt-1 text-sm text-muted-foreground">
                No organizations available yet.
              </p>
            </div>
          </div>
        </section>

        <section
          aria-labelledby="participations-heading"
          className="rounded-xl border bg-card p-5"
        >
          <div className="flex items-start gap-4">
            <div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-muted">
              <ClipboardList
                aria-hidden="true"
                className="size-5"
              />
            </div>

            <div>
              <h2
                id="participations-heading"
                className="font-medium"
              >
                Participations
              </h2>

              <p className="mt-1 text-sm text-muted-foreground">
                No participations available yet.
              </p>
            </div>
          </div>
        </section>
      </div>
    </div>
  );
}
