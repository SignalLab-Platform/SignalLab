import { LocalNavigation } from "@/components/navigation/local-navigation";

import
{
  PresentationContextBar,
} from "@/components/navigation/presentation-context-bar";

import { DEMO_NAVIGATION_DATA } from "@/navigation/demo-navigation-data";

import
{
  CAMPAIGN_VIEW_DEFINITIONS,
  getNavigationViewLabel,
} from "@/navigation/navigation-view-definitions";

import type
{
  CampaignNavigationLocation,
} from "@/navigation/navigation-types";

type WorkspacePresentationProps =
{
  location: CampaignNavigationLocation;
};

export function WorkspacePresentation(
  { location }: Readonly<WorkspacePresentationProps>
)
{
  const title = location.campaignId === DEMO_NAVIGATION_DATA.campaign.id
    ? DEMO_NAVIGATION_DATA.campaign.label
    : location.campaignId;

  const viewLabel = getNavigationViewLabel(
    CAMPAIGN_VIEW_DEFINITIONS,
    location.localView
  );

  const status = (
    <span className="rounded-full border bg-muted px-2.5 py-1 text-xs font-medium text-muted-foreground">
      Demo
    </span>
  );

  return (
    <section className="min-h-full w-full">
      <PresentationContextBar
        variant="workspace"
        contextType="Campaign"
        contextLabel={title}
        navigation={<LocalNavigation location={location} />}
        actions={status}
      />

      <div className="mx-auto w-full max-w-6xl px-8 py-6">
        <div className="rounded-xl border bg-card p-6 text-card-foreground">
          <p className="text-xs font-semibold uppercase tracking-[0.16em] text-primary">
            {viewLabel}
          </p>

          <h2 className="mt-2 text-xl font-semibold">
            {viewLabel} view
          </h2>

          <p className="mt-3 max-w-2xl text-sm leading-6 text-muted-foreground">
            This structural placeholder belongs to the current Campaign
            Workspace. Future milestones will implement the business content
            without changing its identity or Local Navigation.
          </p>
        </div>
      </div>
    </section>
  );
}
