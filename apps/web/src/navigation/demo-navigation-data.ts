type DemoNavigationResource =
{
  id: string;
  label: string;
};

type DemoProjectNavigationResource = DemoNavigationResource &
{
  organizationId: string;
};

type DemoCampaignNavigationResource = DemoNavigationResource &
{
  organizationId: string;
  projectId: string;
};

export const DEMO_NAVIGATION_DATA =
{
  organization:
  {
    id: "demo-organization",
    label: "Demo Organization",
  },

  project:
  {
    id: "demo-project",
    label: "Demo Project",
    organizationId: "demo-organization",
  },

  campaign:
  {
    id: "demo-campaign",
    label: "Demo Campaign",
    organizationId: "demo-organization",
    projectId: "demo-project",
  },

  participation:
  {
    id: "demo-participation",
    label: "Demo Participation",
  },
} as const satisfies
{
  organization: DemoNavigationResource;
  project: DemoProjectNavigationResource;
  campaign: DemoCampaignNavigationResource;
  participation: DemoNavigationResource;
};
