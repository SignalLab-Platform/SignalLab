"use client";

import
{
  Building2,
  ChevronDown,
  ChevronRight,
  Blocks,
  House,
  Search,
} from "lucide-react";

import { useState } from "react";

import type
{
  LucideIcon,
} from "lucide-react";

import Link from "next/link";
import { usePathname } from "next/navigation";

import { useOrganizations } from "@/features/organizations/use-organizations";

import
{
  getPersistedLocalView,
  useNavigationState,
} from "@/navigation/navigation-state";

import
{
  HOME_ROUTE,
  buildOrganizationRoute,
} from "@/navigation/navigation-routes";

import
{
  resolveNavigationLocation,
} from "@/navigation/navigation-resolver";

import
{
  ORGANIZATION_VIEWS,
} from "@/navigation/navigation-types";

import type
{
  NavigationContextKey,
  NavigationLocation,
} from "@/navigation/navigation-types";

type ContextNavigationKind =
  | "home"
  | "organization";

type ContextNavigationStatus =
  | "current"
  | "available";

type ContextNavigationItem =
{
  kind: ContextNavigationKind;
  label: string;
  href: string;
  icon: LucideIcon;
  organizationId?: string;
};

function buildOrganizationContextKey(
  organizationId: string
): NavigationContextKey
{
  return `organization:${organizationId}`;
}

function getContextNavigationStatus(
  location: NavigationLocation | null,
  item: ContextNavigationItem
): ContextNavigationStatus
{
  if (location === null)
  {
    return "available";
  }

  switch (item.kind)
  {
    case "home":
    {
      return location.contextKind === "home"
        ? "current"
        : "available";
    }

    case "organization":
    {
      if (
        (
          location.contextKind === "organization" ||
          location.contextKind === "organization-administration" ||
          location.contextKind === "project" ||
          location.contextKind === "campaign"
        ) &&
        location.organizationId === item.organizationId
      )
      {
        return "current";
      }

      return "available";
    }
  }
}

function getItemClassName(
  status: ContextNavigationStatus
): string
{
  const baseClassName =
    "relative flex min-w-0 items-center gap-3 rounded-lg border border-transparent px-3 py-2.5 text-sm transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-sidebar-ring";

  if (status === "current")
  {
    return `${baseClassName} border-primary/40 bg-sidebar-accent text-sidebar-accent-foreground shadow-sm before:absolute before:inset-y-2 before:left-0 before:w-1 before:rounded-full before:bg-primary`;
  }

  return `${baseClassName} text-sidebar-foreground hover:bg-sidebar-accent/60 hover:text-sidebar-accent-foreground`;
}

function renderContextNavigationItem(
  item: ContextNavigationItem,
  location: NavigationLocation | null
): React.ReactNode
{
  const status = getContextNavigationStatus(
    location,
    item
  );

  const Icon = item.icon;

  return (
    <Link
      key={item.href}
      href={item.href}
      scroll={false}
      title={item.label}
      aria-current={
        status === "current"
          ? "location"
          : undefined
      }
      data-context-status={status}
      className={getItemClassName(status)}
    >
      <Icon
        aria-hidden="true"
        className="size-4 shrink-0"
      />

      <span
        data-sidebar-expanded-only
        className="min-w-0 truncate"
      >
        {item.label}
      </span>
    </Link>
  );
}


type OrganizationNavigationItemProps =
{
  item: ContextNavigationItem;
  location: NavigationLocation | null;
  isExpanded: boolean;
  onToggle: (organizationId: string) => void;
};

function OrganizationNavigationItem(
  props: OrganizationNavigationItemProps
)
{
  const {
    item,
    location,
    isExpanded,
    onToggle,
  } = props;

  const organizationId = item.organizationId;

  if (!organizationId)
  {
    return null;
  }

  const status = getContextNavigationStatus(location, item);

  return (
    <div className="min-w-0">
      <div className="flex min-w-0 items-center gap-1">
        <button
          type="button"
          aria-label={`${isExpanded ? "Collapse" : "Expand"} ${item.label}`}
          aria-expanded={isExpanded}
          aria-controls={`organization-children-${organizationId}`}
          onClick={() =>
          {
            onToggle(organizationId);
          }}
          className="inline-flex size-8 shrink-0 items-center justify-center rounded-lg text-muted-foreground transition-colors hover:bg-sidebar-accent hover:text-sidebar-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-sidebar-ring"
        >
          {isExpanded
            ? <ChevronDown aria-hidden="true" className="size-4" />
            : <ChevronRight aria-hidden="true" className="size-4" />}
        </button>

        <Link
          href={item.href}
          scroll={false}
          title={item.label}
          aria-current={status === "current" ? "location" : undefined}
          data-context-status={status}
          className={`${getItemClassName(status)} flex-1 px-2`}
        >
          <Building2
            aria-hidden="true"
            className="size-4 shrink-0"
          />

          <span
            data-sidebar-expanded-only
            className="min-w-0 truncate"
          >
            {item.label}
          </span>
        </Link>
      </div>

      <div
        id={`organization-children-${organizationId}`}
        hidden={!isExpanded}
        data-sidebar-expanded-only
        className="ml-9 border-l border-sidebar-border pl-3"
      >
        {isExpanded && (
          <div className="flex items-center gap-2 py-2 text-xs text-muted-foreground">
            <Blocks aria-hidden="true" className="size-3.5 shrink-0" />
            <span>No projects yet.</span>
          </div>
        )}
      </div>
    </div>
  );
}

export function ContextNavigation()
{
  const pathname = usePathname();
  const location = resolveNavigationLocation(pathname);
  const navigationState = useNavigationState();
  const organizationsQuery = useOrganizations();
  const [sidebarFilter, setSidebarFilter] = useState("");

  const activeOrganizationId =
    location?.contextKind === "organization" ||
    location?.contextKind === "organization-administration" ||
    location?.contextKind === "project" ||
    location?.contextKind === "campaign"
      ? location.organizationId
      : null;

  type OrganizationExpansionState =
  {
    expandedIds: Set<string>;
    navigationKey: string;
  };

  const navigationKey = location?.contextKey ?? pathname;

  const [expansionState, setExpansionState] =
    useState<OrganizationExpansionState>(() =>
    {
      return {
        expandedIds: activeOrganizationId
          ? new Set([activeOrganizationId])
          : new Set<string>(),
        navigationKey,
      };
    });

  let currentExpansionState = expansionState;

  if (expansionState.navigationKey !== navigationKey)
  {
    const nextExpandedIds = new Set(expansionState.expandedIds);

    if (activeOrganizationId)
    {
      nextExpandedIds.add(activeOrganizationId);
    }

    currentExpansionState =
    {
      expandedIds: nextExpandedIds,
      navigationKey,
    };

    setExpansionState(currentExpansionState);
  }

  function toggleOrganization(organizationId: string): void
  {
    setExpansionState((previous) =>
    {
      const nextExpandedIds = new Set(previous.expandedIds);

      if (nextExpandedIds.has(organizationId))
      {
        nextExpandedIds.delete(organizationId);
      }
      else
      {
        nextExpandedIds.add(organizationId);
      }

      return {
        ...previous,
        expandedIds: nextExpandedIds,
      };
    });
  }

  const homeItem: ContextNavigationItem =
  {
    kind: "home",
    label: "Home",
    href: HOME_ROUTE,
    icon: House,
  };

  const workspaceItems: ContextNavigationItem[] =
    (organizationsQuery.data ?? []).map((organization) =>
    {
      const organizationView = getPersistedLocalView(
        navigationState,
        buildOrganizationContextKey(organization.id),
        ORGANIZATION_VIEWS,
        "all"
      );

      return {
        kind: "organization",
        label: organization.name,
        href: buildOrganizationRoute(
          organization.id,
          organizationView
        ),
        icon: Building2,
        organizationId: organization.id,
      };
    });

  const normalizedSidebarFilter = sidebarFilter.trim().toLocaleLowerCase();

  const filteredWorkspaceItems = workspaceItems.filter((item) =>
  {
    return item.label.toLocaleLowerCase().includes(normalizedSidebarFilter);
  });

  return (
    <nav
      aria-label="Context navigation"
      className="flex min-h-0 flex-1 flex-col overflow-y-auto p-2"
    >
      <div className="space-y-1">
        {renderContextNavigationItem(
          homeItem,
          location
        )}
      </div>

      <div
        data-sidebar-expanded-only
        className="mt-3 px-1"
      >
        <div className="relative">
          <Search
            aria-hidden="true"
            className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground"
          />

          <input
            type="search"
            aria-label="Filter workspaces"
            placeholder="Filter workspaces..."
            value={sidebarFilter}
            onChange={(event) =>
            {
              setSidebarFilter(event.target.value);
            }}
            className="h-9 w-full rounded-lg border border-sidebar-border bg-sidebar-accent/40 pl-9 pr-3 text-sm text-sidebar-foreground outline-none placeholder:text-muted-foreground focus-visible:ring-2 focus-visible:ring-sidebar-ring"
          />
        </div>
      </div>

      <div className="mt-3 space-y-1 border-t border-sidebar-border pt-3">
        <p
          data-sidebar-expanded-only
          className="px-3 pb-1 text-[0.65rem] font-semibold uppercase tracking-[0.16em] text-muted-foreground"
        >
          Workspaces
        </p>

        {organizationsQuery.isPending
          ? (
            <p
              data-sidebar-expanded-only
              role="status"
              className="px-3 py-2 text-xs text-muted-foreground"
            >
              Loading workspaces...
            </p>
          )
          : organizationsQuery.isError
            ? (
              <p
                data-sidebar-expanded-only
                role="alert"
                className="px-3 py-2 text-xs text-destructive"
              >
                Unable to load workspaces.
              </p>
            )
            : workspaceItems.length === 0
              ? (
                <p
                  data-sidebar-expanded-only
                  className="px-3 py-2 text-xs text-muted-foreground"
                >
                  No workspaces yet.
                </p>
              )
              : filteredWorkspaceItems.length === 0
                ? (
                  <p
                    data-sidebar-expanded-only
                    className="px-3 py-2 text-xs text-muted-foreground"
                  >
                    No matching workspaces.
                  </p>
                )
                : filteredWorkspaceItems.map((item) =>
                  {
                    const organizationId = item.organizationId;

                    if (!organizationId)
                    {
                      return null;
                    }

                    return (
                      <OrganizationNavigationItem
                        key={organizationId}
                        item={item}
                        location={location}
                        isExpanded={currentExpansionState.expandedIds.has(organizationId)}
                        onToggle={toggleOrganization}
                      />
                    );
                  })}
      </div>

      <div className="mt-3 space-y-1 border-t border-sidebar-border pt-3">
        <p
          data-sidebar-expanded-only
          className="px-3 pb-1 text-[0.65rem] font-semibold uppercase tracking-[0.16em] text-muted-foreground"
        >
          My participations
        </p>

        <p
          data-sidebar-expanded-only
          className="px-3 py-2 text-xs text-muted-foreground"
        >
          No participations yet.
        </p>
      </div>
    </nav>
  );
}
