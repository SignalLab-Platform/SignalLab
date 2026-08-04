import
{
  buildCampaignRoute,
  buildOrganizationRoute,
  buildParticipationRoute,
  buildProjectRoute,
  HOME_ROUTE,
} from "@/navigation/navigation-routes";

import
{
  CAMPAIGN_VIEWS,
  ORGANIZATION_VIEWS,
  PARTICIPATION_SECTIONS,
  PROJECT_VIEWS,
} from "@/navigation/navigation-types";

import type
{
  NavigationLocation,
} from "@/navigation/navigation-types";

type ParsedPathname =
{
  segments: string[];
};

function isAllowedValue<TValue extends string>(
  value: string,
  allowedValues: readonly TValue[]
): value is TValue
{
  return allowedValues.some((allowedValue) => allowedValue === value);
}

function decodeRouteSegment(segment: string): string | null
{
  if (segment.length === 0)
  {
    return null;
  }

  try
  {
    const decodedSegment = decodeURIComponent(segment);

    if (
      decodedSegment.length === 0 ||
      decodedSegment.trim() !== decodedSegment
    )
    {
      return null;
    }

    return decodedSegment;
  }
  catch
  {
    return null;
  }
}

function parsePathname(pathname: string): ParsedPathname | null
{
  if (!pathname.startsWith("/") || pathname.startsWith("//"))
  {
    return null;
  }

  const pathEndIndex = pathname.search(/[?#]/);
  const pathnameWithoutQuery = pathEndIndex === -1
    ? pathname
    : pathname.slice(0, pathEndIndex);

  const normalizedPathname = pathnameWithoutQuery.length > 1
    ? pathnameWithoutQuery.replace(/\/+$/, "")
    : pathnameWithoutQuery;

  if (normalizedPathname.includes("//"))
  {
    return null;
  }

  if (normalizedPathname === "/")
  {
    return {
      segments: [],
    };
  }

  const encodedSegments = normalizedPathname.slice(1).split("/");
  const decodedSegments: string[] = [];

  for (const encodedSegment of encodedSegments)
  {
    const decodedSegment = decodeRouteSegment(encodedSegment);

    if (decodedSegment === null)
    {
      return null;
    }

    decodedSegments.push(decodedSegment);
  }

  return {
    segments: decodedSegments,
  };
}

function resolveHomeLocation(segments: string[]): NavigationLocation | null
{
  if (segments.length !== 1 || segments[0] !== "home")
  {
    return null;
  }

  return {
    pathname: HOME_ROUTE,
    contextKind: "home",
    contextKey: "home",
    presentationModel: "personal-hub",
    localView: null,
    hierarchy: [],
  };
}

function resolveOrganizationLocation(
  segments: string[]
): NavigationLocation | null
{
  if (
    segments.length !== 3 ||
    segments[0] !== "organizations"
  )
  {
    return null;
  }

  const organizationId = segments[1];
  const view = segments[2];

  if (!isAllowedValue(view, ORGANIZATION_VIEWS))
  {
    return null;
  }

  return {
    pathname: buildOrganizationRoute(organizationId, view),
    contextKind: "organization",
    contextKey: `organization:${organizationId}`,
    presentationModel: "explorer",
    organizationId,
    localView: view,
    hierarchy:
    [
      {
        kind: "organization",
        id: organizationId,
      },
    ],
  };
}

function resolveProjectLocation(
  segments: string[]
): NavigationLocation | null
{
  if (
    segments.length !== 5 ||
    segments[0] !== "organizations" ||
    segments[2] !== "projects"
  )
  {
    return null;
  }

  const organizationId = segments[1];
  const projectId = segments[3];
  const view = segments[4];

  if (!isAllowedValue(view, PROJECT_VIEWS))
  {
    return null;
  }

  return {
    pathname: buildProjectRoute(organizationId, projectId, view),
    contextKind: "project",
    contextKey: `project:${projectId}`,
    presentationModel: "explorer",
    organizationId,
    projectId,
    localView: view,
    hierarchy:
    [
      {
        kind: "organization",
        id: organizationId,
      },
      {
        kind: "project",
        id: projectId,
      },
    ],
  };
}

function resolveCampaignLocation(
  segments: string[]
): NavigationLocation | null
{
  if (
    segments.length !== 7 ||
    segments[0] !== "organizations" ||
    segments[2] !== "projects" ||
    segments[4] !== "campaigns"
  )
  {
    return null;
  }

  const organizationId = segments[1];
  const projectId = segments[3];
  const campaignId = segments[5];
  const view = segments[6];

  if (!isAllowedValue(view, CAMPAIGN_VIEWS))
  {
    return null;
  }

  return {
    pathname: buildCampaignRoute(
      organizationId,
      projectId,
      campaignId,
      view
    ),
    contextKind: "campaign",
    contextKey: `campaign:${campaignId}`,
    presentationModel: "workspace",
    organizationId,
    projectId,
    campaignId,
    localView: view,
    hierarchy:
    [
      {
        kind: "organization",
        id: organizationId,
      },
      {
        kind: "project",
        id: projectId,
      },
      {
        kind: "campaign",
        id: campaignId,
      },
    ],
  };
}

function resolveParticipationLocation(
  segments: string[]
): NavigationLocation | null
{
  if (
    segments.length !== 3 ||
    segments[0] !== "participations"
  )
  {
    return null;
  }

  const participationId = segments[1];
  const section = segments[2];

  if (!isAllowedValue(section, PARTICIPATION_SECTIONS))
  {
    return null;
  }

  return {
    pathname: buildParticipationRoute(participationId, section),
    contextKind: "participation",
    contextKey: `participation:${participationId}`,
    presentationModel: "document",
    participationId,
    localView: section,
    hierarchy:
    [
      {
        kind: "participation",
        id: participationId,
      },
    ],
  };
}

export function resolveNavigationLocation(
  pathname: string
): NavigationLocation | null
{
  const parsedPathname = parsePathname(pathname);

  if (parsedPathname === null)
  {
    return null;
  }

  const resolvers =
  [
    resolveHomeLocation,
    resolveOrganizationLocation,
    resolveProjectLocation,
    resolveCampaignLocation,
    resolveParticipationLocation,
  ];

  for (const resolver of resolvers)
  {
    const location = resolver(parsedPathname.segments);

    if (location !== null)
    {
      return location;
    }
  }

  return null;
}
