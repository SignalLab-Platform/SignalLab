import type
{
  CampaignView,
  OrganizationView,
  ParticipationSection,
  ProjectView,
} from "@/navigation/navigation-types";

export const HOME_ROUTE = "/home";

export const ANALYSIS_OVERLAY_QUERY_KEY = "overlay";
export const ANALYSIS_OVERLAY_QUERY_VALUE = "analysis";

const INTERNAL_URL_BASE = "https://signallab.local";

function encodeRouteSegment(value: string, parameterName: string): string
{
  const normalizedValue = value.trim();

  if (normalizedValue.length === 0)
  {
    throw new Error(`${parameterName} cannot be empty.`);
  }

  return encodeURIComponent(normalizedValue);
}

export function buildOrganizationRoute(
  organizationId: string,
  view: OrganizationView
): string
{
  const encodedOrganizationId = encodeRouteSegment(
    organizationId,
    "organizationId"
  );

  return `/organizations/${encodedOrganizationId}/${view}`;
}

export function buildProjectRoute(
  organizationId: string,
  projectId: string,
  view: ProjectView
): string
{
  const encodedOrganizationId = encodeRouteSegment(
    organizationId,
    "organizationId"
  );

  const encodedProjectId = encodeRouteSegment(
    projectId,
    "projectId"
  );

  return `/organizations/${encodedOrganizationId}/projects/${encodedProjectId}/${view}`;
}

export function buildCampaignRoute(
  organizationId: string,
  projectId: string,
  campaignId: string,
  view: CampaignView
): string
{
  const encodedOrganizationId = encodeRouteSegment(
    organizationId,
    "organizationId"
  );

  const encodedProjectId = encodeRouteSegment(
    projectId,
    "projectId"
  );

  const encodedCampaignId = encodeRouteSegment(
    campaignId,
    "campaignId"
  );

  return `/organizations/${encodedOrganizationId}/projects/${encodedProjectId}/campaigns/${encodedCampaignId}/${view}`;
}

export function buildParticipationRoute(
  participationId: string,
  section: ParticipationSection
): string
{
  const encodedParticipationId = encodeRouteSegment(
    participationId,
    "participationId"
  );

  return `/participations/${encodedParticipationId}/${section}`;
}

export function buildAnalysisOverlayRoute(route: string): string
{
  const url = new URL(
    route,
    "https://signallab.local"
  );

  url.searchParams.set(
    ANALYSIS_OVERLAY_QUERY_KEY,
    ANALYSIS_OVERLAY_QUERY_VALUE
  );

  return `${url.pathname}${url.search}${url.hash}`;
}

export function removeAnalysisOverlayFromRoute(
  route: string
): string
{
  const url = new URL(
    route,
    "https://signallab.local"
  );

  url.searchParams.delete(
    ANALYSIS_OVERLAY_QUERY_KEY
  );

  return `${url.pathname}${url.search}${url.hash}`;
}
