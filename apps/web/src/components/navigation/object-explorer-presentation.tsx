"use client";

import
{
  Building2,
  Blocks,
  Plus,
  Search,
} from "lucide-react";

import 
{ 
  LocalNavigation,
} from "@/components/navigation/local-navigation";

import 
{ 
  ContextBreadcrumb,
} from "@/components/navigation/context-breadcrumb";

import
{
  OrganizationAdministrationLink,
} from "@/components/navigation/organization-administration-link";

import
{
  buildOrganizationRoute,
} from "@/navigation/navigation-routes";

import
{
  PresentationContextBar,
} from "@/components/navigation/presentation-context-bar";

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

import 
{
  useOrganizations
} from "@/features/organizations/use-organizations";

type ObjectExplorerLocation =
  | OrganizationNavigationLocation
  | ProjectNavigationLocation;

type ObjectExplorerPresentationProps =
{
  location: ObjectExplorerLocation;
};

export function ObjectExplorerPresentation(
  { location }: Readonly<ObjectExplorerPresentationProps>
)
{
  const navigationState = useNavigationState();

  const organizationsQuery = useOrganizations();

  const organization = organizationsQuery.data?.find((item) =>
  {
    return item.id === location.organizationId;
  });

  let title: string;

  if (location.contextKind === "organization")
  {
    if (organization)
    {
      title = organization.name;
    }
    else if (organizationsQuery.isPending)
    {
      title = "Loading...";
    }
    else
    {
      title = "Organization unavailable";
    }
  }
  else
  {
    title = "Project";
  }

  const searchQuery =
    navigationState
      .resources[location.contextKey]
      ?.explorerSearchQuery ?? "";

  const breadcrumbItems = location.contextKind === "organization"
    ? [
        {
          label: title,
          icon: Building2,
          action: (
            <OrganizationAdministrationLink
              organizationId={location.organizationId}
              organizationName={title}
            />
          ),
        },
      ]
    : [
        {
          label: organization?.name ?? location.organizationId,
          href: buildOrganizationRoute(location.organizationId, "all"),
          icon: Building2,
          action: (
            <OrganizationAdministrationLink
              organizationId={location.organizationId}
              organizationName={organization?.name ?? location.organizationId}
            />
          ),
        },
        {
          label: title,
          icon: Blocks,
        },
      ];

  const backHref = location.contextKind === "organization"
    ? "/home"
    : buildOrganizationRoute(location.organizationId, "all");

  const actions = (
    <>
      <div className="relative w-60">
        <Search
          aria-hidden="true"
          className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground"
        />

        <input
          type="search"
          disabled
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
        breadcrumb={
          <ContextBreadcrumb
            items={breadcrumbItems}
            backHref={backHref}
          />
        }
        navigation={<LocalNavigation location={location} />}
        actions={actions}
      />

      <div className="mx-auto w-full max-w-6xl px-8 py-6">
        <div className="rounded-xl border border-dashed bg-card p-8 text-center">
          <p className="font-medium">
            No resources to display
          </p>

          <p className="mt-2 text-sm text-muted-foreground">
            Resources will become available as their corresponding
            features are implemented.
          </p>
        </div>
      </div>
    </section>
  );
}