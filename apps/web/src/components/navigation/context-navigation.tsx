"use client";

import
{
  Building2,
  ClipboardList,
  Folder,
  House,
  Megaphone,
} from "lucide-react";

import type
{
  LucideIcon,
} from "lucide-react";

import Link from "next/link";
import { usePathname } from "next/navigation";

import { DEMO_NAVIGATION_DATA } from "@/navigation/demo-navigation-data";

import
{
  getPersistedLocalView,
  useNavigationState,
} from "@/navigation/navigation-state";

import
{
  HOME_ROUTE,
  buildCampaignRoute,
  buildOrganizationRoute,
  buildParticipationRoute,
  buildProjectRoute,
} from "@/navigation/navigation-routes";

import
{
  resolveNavigationLocation,
} from "@/navigation/navigation-resolver";

import
{
  CAMPAIGN_VIEWS,
  ORGANIZATION_VIEWS,
  PARTICIPATION_SECTIONS,
  PROJECT_VIEWS,
} from "@/navigation/navigation-types";

import type
{
  NavigationContextKey,
  NavigationLocation,
} from "@/navigation/navigation-types";

type ContextNavigationKind =
  | "home"
  | "organization"
  | "project"
  | "campaign"
  | "participation";

type ContextNavigationStatus =
  | "current"
  | "ancestor"
  | "available";

type ContextNavigationItem =
{
  kind: ContextNavigationKind;
  label: string;
  href: string;
  icon: LucideIcon;
  organizationId?: string;
  projectId?: string;
  campaignId?: string;
  participationId?: string;
};

function buildOrganizationContextKey(
  organizationId: string
): NavigationContextKey
{
  return `organization:${organizationId}`;
}

function buildProjectContextKey(
  projectId: string
): NavigationContextKey
{
  return `project:${projectId}`;
}

function buildCampaignContextKey(
  campaignId: string
): NavigationContextKey
{
  return `campaign:${campaignId}`;
}

function buildParticipationContextKey(
  participationId: string
): NavigationContextKey
{
  return `participation:${participationId}`;
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
        location.contextKind === "organization" &&
        location.organizationId === item.organizationId
      )
      {
        return "current";
      }

      if (
        (
          location.contextKind === "project" ||
          location.contextKind === "campaign"
        ) &&
        location.organizationId === item.organizationId
      )
      {
        return "ancestor";
      }

      return "available";
    }

    case "project":
    {
      if (
        location.contextKind === "project" &&
        location.projectId === item.projectId
      )
      {
        return "current";
      }

      if (
        location.contextKind === "campaign" &&
        location.projectId === item.projectId
      )
      {
        return "ancestor";
      }

      return "available";
    }

    case "campaign":
    {
      return (
        location.contextKind === "campaign" &&
        location.campaignId === item.campaignId
      )
        ? "current"
        : "available";
    }

    case "participation":
    {
      return (
        location.contextKind === "participation" &&
        location.participationId === item.participationId
      )
        ? "current"
        : "available";
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

  if (status === "ancestor")
  {
    return `${baseClassName} text-sidebar-foreground ring-1 ring-inset ring-sidebar-foreground/25 hover:bg-sidebar-accent/60`;
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
      key={item.kind}
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

export function ContextNavigation()
{
  const pathname = usePathname();
  const location = resolveNavigationLocation(pathname);
  const navigationState = useNavigationState();

  const organization = DEMO_NAVIGATION_DATA.organization;
  const project = DEMO_NAVIGATION_DATA.project;
  const campaign = DEMO_NAVIGATION_DATA.campaign;
  const participation = DEMO_NAVIGATION_DATA.participation;

  const organizationView = getPersistedLocalView(
    navigationState,
    buildOrganizationContextKey(organization.id),
    ORGANIZATION_VIEWS,
    "all"
  );

  const projectView = getPersistedLocalView(
    navigationState,
    buildProjectContextKey(project.id),
    PROJECT_VIEWS,
    "all"
  );

  const campaignView = getPersistedLocalView(
    navigationState,
    buildCampaignContextKey(campaign.id),
    CAMPAIGN_VIEWS,
    "overview"
  );

  const participationSection =
    getPersistedLocalView(
      navigationState,
      buildParticipationContextKey(
        participation.id
      ),
      PARTICIPATION_SECTIONS,
      "overview"
    );

  const homeItem: ContextNavigationItem =
  {
    kind: "home",
    label: "Home",
    href: HOME_ROUTE,
    icon: House,
  };

  const workspaceItems: ContextNavigationItem[] =
  [
    {
      kind: "organization",
      label: organization.label,
      href: buildOrganizationRoute(
        organization.id,
        organizationView
      ),
      icon: Building2,
      organizationId: organization.id,
    },
    {
      kind: "project",
      label: project.label,
      href: buildProjectRoute(
        organization.id,
        project.id,
        projectView
      ),
      icon: Folder,
      organizationId: organization.id,
      projectId: project.id,
    },
    {
      kind: "campaign",
      label: campaign.label,
      href: buildCampaignRoute(
        organization.id,
        project.id,
        campaign.id,
        campaignView
      ),
      icon: Megaphone,
      organizationId: organization.id,
      projectId: project.id,
      campaignId: campaign.id,
    },
  ];

  const participationItem: ContextNavigationItem =
  {
    kind: "participation",
    label: participation.label,
    href: buildParticipationRoute(
      participation.id,
      participationSection
    ),
    icon: ClipboardList,
    participationId: participation.id,
  };

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

      <div className="mt-3 space-y-1 border-t border-sidebar-border pt-3">
        <p
          data-sidebar-expanded-only
          className="px-3 pb-1 text-[0.65rem] font-semibold uppercase tracking-[0.16em] text-muted-foreground"
        >
          Workspaces
        </p>

        {workspaceItems.map((item) =>
        {
          return renderContextNavigationItem(
            item,
            location
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

        {renderContextNavigationItem(
          participationItem,
          location
        )}
      </div>
    </nav>
  );
}
