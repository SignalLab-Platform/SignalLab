"use client";

import
{
  Folder,
  Plus,
  Search,
} from "lucide-react";

import Link from "next/link";

import { LocalNavigation } from "@/components/navigation/local-navigation";

import
{
  PresentationContextBar,
} from "@/components/navigation/presentation-context-bar";

import { DEMO_NAVIGATION_DATA } from "@/navigation/demo-navigation-data";

import
{
  buildCampaignRoute,
  buildProjectRoute,
} from "@/navigation/navigation-routes";

import type
{
  OrganizationNavigationLocation,
  ProjectNavigationLocation,
} from "@/navigation/navigation-types";

import
{
  updateNavigationResourceState,
  useNavigationState,
} from "@/navigation/navigation-state";

type ExplorerLocation =
  | OrganizationNavigationLocation
  | ProjectNavigationLocation;

type ExplorerPresentationProps =
{
  location: ExplorerLocation;
};

type ExplorerResource =
{
  type: string;
  label: string;
  href: string;
};

function getExplorerContextType(location: ExplorerLocation): string
{
  return location.contextKind === "organization"
    ? "Organization"
    : "Project";
}

function getExplorerTitle(location: ExplorerLocation): string
{
  if (location.contextKind === "organization")
  {
    return location.organizationId === DEMO_NAVIGATION_DATA.organization.id
      ? DEMO_NAVIGATION_DATA.organization.label
      : location.organizationId;
  }

  return location.projectId === DEMO_NAVIGATION_DATA.project.id
    ? DEMO_NAVIGATION_DATA.project.label
    : location.projectId;
}

function getExplorerResources(location: ExplorerLocation): ExplorerResource[]
{
  if (
    location.contextKind === "organization" &&
    (location.localView === "all" || location.localView === "projects")
  )
  {
    return [
      {
        type: "Project",
        label: DEMO_NAVIGATION_DATA.project.label,
        href: buildProjectRoute(
          location.organizationId,
          DEMO_NAVIGATION_DATA.project.id,
          "all"
        ),
      },
    ];
  }

  if (
    location.contextKind === "project" &&
    (location.localView === "all" || location.localView === "campaigns")
  )
  {
    return [
      {
        type: "Campaign",
        label: DEMO_NAVIGATION_DATA.campaign.label,
        href: buildCampaignRoute(
          location.organizationId,
          location.projectId,
          DEMO_NAVIGATION_DATA.campaign.id,
          "overview"
        ),
      },
    ];
  }

  return [];
}

export function ExplorerPresentation(
  { location }: Readonly<ExplorerPresentationProps>
)
{
  const navigationState = useNavigationState();

  const searchQuery =
    navigationState
      .resources[location.contextKey]
      ?.explorerSearchQuery ?? "";

  const contextType = getExplorerContextType(location);
  const title = getExplorerTitle(location);
  const resources = getExplorerResources(location);
  const normalizedQuery = searchQuery.trim().toLowerCase();

  const filteredResources = resources.filter((resource) =>
  {
    return resource.label.toLowerCase().includes(normalizedQuery);
  });

  const actions = (
    <>
      <div className="relative w-60">
        <Search
          aria-hidden="true"
          className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground"
        />

        <input
          type="search"
          value={searchQuery}
          aria-label={`Search ${title}`}
          placeholder="Search..."
          onChange={(event) =>
          {
            updateNavigationResourceState(
              location.contextKey,
              {
                explorerSearchQuery: event.target.value,
              }
            );
          }}
          className="h-9 w-full rounded-lg border bg-background pl-9 pr-3 text-sm outline-none placeholder:text-muted-foreground focus-visible:ring-2 focus-visible:ring-ring"
        />
      </div>

      <button
        type="button"
        disabled
        title="Resource creation will be introduced by a later milestone."
        className="inline-flex h-9 items-center gap-2 rounded-lg bg-primary px-3 text-sm font-medium text-primary-foreground opacity-55"
      >
        <Plus
          aria-hidden="true"
          className="size-4"
        />

        Create resource
      </button>
    </>
  );

  return (
    <section className="min-h-full w-full">
      <PresentationContextBar
        variant="explorer"
        contextType={contextType}
        contextLabel={title}
        navigation={<LocalNavigation location={location} />}
        actions={actions}
      />

      <div className="mx-auto w-full max-w-6xl px-8 py-6">
        <p className="mb-4 text-sm text-muted-foreground">
          Resources in {title}
        </p>

        {filteredResources.length === 0
          ? (
            <div className="rounded-xl border border-dashed bg-card p-8 text-center">
              <p className="font-medium">
                No resources to display
              </p>

              <p className="mt-2 text-sm text-muted-foreground">
                This Explorer keeps its Search, tabs, and Primary Actions even
                when the current view is empty.
              </p>
            </div>
          )
          : (
            <div className="grid gap-3">
              {filteredResources.map((resource) =>
              {
                return (
                  <Link
                    key={resource.href}
                    href={resource.href}
                    scroll={false}
                    className="flex items-center gap-4 rounded-xl border bg-card p-4 text-card-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
                  >
                    <div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
                      <Folder
                        aria-hidden="true"
                        className="size-5"
                      />
                    </div>

                    <div className="min-w-0">
                      <p className="text-xs font-semibold uppercase tracking-[0.14em] text-muted-foreground">
                        {resource.type}
                      </p>

                      <p className="truncate font-medium">
                        {resource.label}
                      </p>
                    </div>
                  </Link>
                );
              })}
            </div>
          )}
      </div>
    </section>
  );
}
