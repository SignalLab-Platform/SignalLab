import type
{
  CampaignView,
  OrganizationView,
  ParticipationSection,
  ProjectView,
} from "@/navigation/navigation-types";

export type NavigationViewDefinition<TView extends string> =
{
  value: TView;
  label: string;
};

export const ORGANIZATION_VIEW_DEFINITIONS: readonly NavigationViewDefinition<OrganizationView>[] =
[
  {
    value: "all",
    label: "All",
  },
  {
    value: "projects",
    label: "Projects",
  },
  {
    value: "measures",
    label: "Measures",
  },
  {
    value: "form-templates",
    label: "Form Templates",
  },
];

export const PROJECT_VIEW_DEFINITIONS: readonly NavigationViewDefinition<ProjectView>[] =
[
  {
    value: "all",
    label: "All",
  },
  {
    value: "campaigns",
    label: "Campaigns",
  },
  {
    value: "builds",
    label: "Builds",
  },
  {
    value: "analyses",
    label: "Analyses",
  },
];

export const CAMPAIGN_VIEW_DEFINITIONS: readonly NavigationViewDefinition<CampaignView>[] =
[
  {
    value: "overview",
    label: "Overview",
  },
  {
    value: "configuration",
    label: "Configuration",
  },
  {
    value: "form",
    label: "Form",
  },
  {
    value: "recruitment",
    label: "Recruitment",
  },
  {
    value: "participants",
    label: "Participants",
  },
  {
    value: "results",
    label: "Results",
  },
  {
    value: "settings",
    label: "Settings",
  },
];

export const PARTICIPATION_SECTION_DEFINITIONS: readonly NavigationViewDefinition<ParticipationSection>[] =
[
  {
    value: "overview",
    label: "Overview",
  },
  {
    value: "consent",
    label: "Consent",
  },
  {
    value: "activities",
    label: "Activities",
  },
  {
    value: "progress",
    label: "Progress",
  },
  {
    value: "support",
    label: "Support",
  },
];

export function getNavigationViewLabel<TView extends string>(
  definitions: readonly NavigationViewDefinition<TView>[],
  value: TView
): string
{
  const definition = definitions.find((item) => item.value === value);

  return definition?.label ?? value;
}
