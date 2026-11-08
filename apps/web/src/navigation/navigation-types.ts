export const ORGANIZATION_VIEWS =
[
  "all",
  "projects",
  "measures",
  "form-templates",
] as const;

export const ORGANIZATION_ADMINISTRATION_VIEWS =
[
  "members",
  "invitations",
  "settings",
] as const;

export type OrganizationAdministrationView =
  (typeof ORGANIZATION_ADMINISTRATION_VIEWS)[number];

export const PROJECT_VIEWS =
[
  "all",
  "campaigns",
  "builds",
  "analyses",
] as const;

export const CAMPAIGN_VIEWS =
[
  "overview",
  "configuration",
  "form",
  "recruitment",
  "participants",
  "results",
  "settings",
] as const;

export const PARTICIPATION_SECTIONS =
[
  "overview",
  "consent",
  "activities",
  "progress",
  "support",
] as const;

export type OrganizationView = (typeof ORGANIZATION_VIEWS)[number];
export type ProjectView = (typeof PROJECT_VIEWS)[number];
export type CampaignView = (typeof CAMPAIGN_VIEWS)[number];
export type ParticipationSection = (typeof PARTICIPATION_SECTIONS)[number];

export type NavigationContextKind =
  | "home"
  | "organization"
  | "organization-administration"
  | "project"
  | "campaign"
  | "participation";

export type NavigationResourceKind =
  | "organization"
  | "project"
  | "campaign"
  | "participation";

export type NavigationPresentationModel =
  | "personal-hub"
  | "personal-explorer"
  | "object-explorer"
  | "organization-administration"
  | "workspace"
  | "document";

export type NavigationOverlayKind = "analysis";

export type NavigationContextKey =
  | "home"
  | `organization:${string}`
  | `organization-administration:${string}`
  | `project:${string}`
  | `campaign:${string}`
  | `participation:${string}`;

export type NavigationHierarchyItem =
{
  kind: NavigationResourceKind;
  id: string;
};

type BaseNavigationLocation =
{
  pathname: string;
  contextKey: NavigationContextKey;
  hierarchy: readonly NavigationHierarchyItem[];
};

export type HomeNavigationLocation = BaseNavigationLocation &
{
  contextKind: "home";
  presentationModel: "personal-hub";
  localView: null;
};

export type OrganizationNavigationLocation = BaseNavigationLocation &
{
  contextKind: "organization";
  presentationModel: "object-explorer";
  organizationId: string;
  localView: OrganizationView;
};

export type OrganizationAdministrationNavigationLocation =
  BaseNavigationLocation &
{
  contextKind: "organization-administration";
  presentationModel: "organization-administration";
  organizationId: string;
  localView: OrganizationAdministrationView;
};

export type ProjectNavigationLocation = BaseNavigationLocation &
{
  contextKind: "project";
  presentationModel: "object-explorer";
  organizationId: string;
  projectId: string;
  localView: ProjectView;
};

export type CampaignNavigationLocation = BaseNavigationLocation &
{
  contextKind: "campaign";
  presentationModel: "workspace";
  organizationId: string;
  projectId: string;
  campaignId: string;
  localView: CampaignView;
};

export type ParticipationNavigationLocation = BaseNavigationLocation &
{
  contextKind: "participation";
  presentationModel: "document";
  participationId: string;
  localView: ParticipationSection;
};

export type NavigationLocation =
  | HomeNavigationLocation
  | OrganizationNavigationLocation
  | OrganizationAdministrationNavigationLocation
  | ProjectNavigationLocation
  | CampaignNavigationLocation
  | ParticipationNavigationLocation;
