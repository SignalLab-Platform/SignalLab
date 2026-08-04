import
{
  Building2,
  ClipboardList,
  UserRound,
} from "lucide-react";

import Link from "next/link";

import { DEMO_NAVIGATION_DATA } from "@/navigation/demo-navigation-data";

import
{
  buildOrganizationRoute,
  buildParticipationRoute,
} from "@/navigation/navigation-routes";

export function PersonalHomePresentation()
{
  return (
    <section className="mx-auto flex min-h-full w-full max-w-5xl flex-col justify-center gap-8 px-8 py-12">
      <div className="max-w-2xl">
        <p className="text-sm font-medium text-primary">
          Personal space
        </p>

        <h1 className="mt-2 text-4xl font-semibold tracking-tight">
          Personal Home
        </h1>

        <p className="mt-4 text-base leading-7 text-muted-foreground">
          Choose the context in which you want to work.
        </p>
      </div>

      <div className="grid gap-4 lg:grid-cols-3">
        <Link
          href={buildOrganizationRoute(
            DEMO_NAVIGATION_DATA.organization.id,
            "all"
          )}
          className="rounded-xl border bg-card p-5 text-card-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
        >
          <Building2
            aria-hidden="true"
            className="size-5 text-primary"
          />

          <h2 className="mt-4 font-semibold">
            {DEMO_NAVIGATION_DATA.organization.label}
          </h2>

          <p className="mt-2 text-sm text-muted-foreground">
            Open the Organization Explorer.
          </p>
        </Link>

        <Link
          href={buildParticipationRoute(
            DEMO_NAVIGATION_DATA.participation.id,
            "overview"
          )}
          className="rounded-xl border bg-card p-5 text-card-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
        >
          <ClipboardList
            aria-hidden="true"
            className="size-5 text-primary"
          />

          <h2 className="mt-4 font-semibold">
            {DEMO_NAVIGATION_DATA.participation.label}
          </h2>

          <p className="mt-2 text-sm text-muted-foreground">
            Open the Participation Document.
          </p>
        </Link>

        <div className="rounded-xl border border-dashed bg-card p-5 text-card-foreground">
          <UserRound
            aria-hidden="true"
            className="size-5 text-muted-foreground"
          />

          <h2 className="mt-4 font-semibold">
            MAP Profile
          </h2>

          <p className="mt-2 text-sm text-muted-foreground">
            This personal resource will be introduced in M10.
          </p>
        </div>
      </div>
    </section>
  );
}
