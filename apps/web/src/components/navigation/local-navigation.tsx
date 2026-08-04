import Link from "next/link";

import
{
  buildCampaignRoute,
  buildOrganizationRoute,
  buildProjectRoute,
} from "@/navigation/navigation-routes";

import
{
  CAMPAIGN_VIEW_DEFINITIONS,
  ORGANIZATION_VIEW_DEFINITIONS,
  PROJECT_VIEW_DEFINITIONS,
} from "@/navigation/navigation-view-definitions";

import type
{
  CampaignNavigationLocation,
  NavigationLocation,
  OrganizationNavigationLocation,
  ProjectNavigationLocation,
} from "@/navigation/navigation-types";

type LocalNavigationItem =
{
  value: string;
  label: string;
  href: string;
};

type LocalNavigationProps =
{
  location: NavigationLocation;
  className?: string;
};

function buildOrganizationItems(
  location: OrganizationNavigationLocation
): LocalNavigationItem[]
{
  return ORGANIZATION_VIEW_DEFINITIONS.map((definition) =>
  {
    return {
      value: definition.value,
      label: definition.label,
      href: buildOrganizationRoute(location.organizationId, definition.value),
    };
  });
}

function buildProjectItems(
  location: ProjectNavigationLocation
): LocalNavigationItem[]
{
  return PROJECT_VIEW_DEFINITIONS.map((definition) =>
  {
    return {
      value: definition.value,
      label: definition.label,
      href: buildProjectRoute(
        location.organizationId,
        location.projectId,
        definition.value
      ),
    };
  });
}

function buildCampaignItems(
  location: CampaignNavigationLocation
): LocalNavigationItem[]
{
  return CAMPAIGN_VIEW_DEFINITIONS.map((definition) =>
  {
    return {
      value: definition.value,
      label: definition.label,
      href: buildCampaignRoute(
        location.organizationId,
        location.projectId,
        location.campaignId,
        definition.value
      ),
    };
  });
}

function buildLocalNavigationItems(
  location: NavigationLocation
): LocalNavigationItem[]
{
  switch (location.contextKind)
  {
    case "organization":
    {
      return buildOrganizationItems(location);
    }

    case "project":
    {
      return buildProjectItems(location);
    }

    case "campaign":
    {
      return buildCampaignItems(location);
    }

    case "home":
    case "participation":
    {
      return [];
    }
  }
}

function getLinkClassName(isCurrent: boolean): string
{
  const baseClassName =
    "inline-flex h-10 items-center border-b-2 px-0.5 text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2";

  if (isCurrent)
  {
    return `${baseClassName} border-primary text-foreground`;
  }

  return `${baseClassName} border-transparent text-muted-foreground hover:border-border hover:text-foreground`;
}

export function LocalNavigation(
  {
    location,
    className,
  }: Readonly<LocalNavigationProps>
)
{
  const items = buildLocalNavigationItems(location);

  if (items.length === 0)
  {
    return null;
  }

  return (
    <nav
      aria-label="Local navigation"
      className={`min-w-0 overflow-x-auto ${className ?? ""}`}
    >
      <div className="flex min-w-max items-center gap-5">
        {items.map((item) =>
        {
          const isCurrent = item.value === location.localView;

          return (
            <Link
              key={item.value}
              href={item.href}
              scroll={false}
              aria-current={isCurrent ? "page" : undefined}
              className={getLinkClassName(isCurrent)}
            >
              {item.label}
            </Link>
          );
        })}
      </div>
    </nav>
  );
}
