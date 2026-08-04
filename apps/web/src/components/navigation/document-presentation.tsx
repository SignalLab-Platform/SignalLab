import
{
  DocumentTableOfContents,
} from "@/components/navigation/document-table-of-contents";

import
{
  PresentationContextBar,
} from "@/components/navigation/presentation-context-bar";

import { DEMO_NAVIGATION_DATA } from "@/navigation/demo-navigation-data";

import
{
  PARTICIPATION_SECTION_DEFINITIONS,
} from "@/navigation/navigation-view-definitions";

import type
{
  ParticipationNavigationLocation,
  ParticipationSection,
} from "@/navigation/navigation-types";

type DocumentPresentationProps =
{
  location: ParticipationNavigationLocation;
};

const SECTION_DESCRIPTIONS: Record<ParticipationSection, string> =
{
  overview: "General information and the current state of this Participation.",
  consent: "Consent information and the participant-facing agreement.",
  activities: "Activities that belong to this Participation flow.",
  progress: "Progress through the Participation and its expected completion.",
  support: "Support information available throughout the Participation.",
};

export function DocumentPresentation(
  { location }: Readonly<DocumentPresentationProps>
)
{
  const title = location.participationId === DEMO_NAVIGATION_DATA.participation.id
    ? DEMO_NAVIGATION_DATA.participation.label
    : location.participationId;

  const status = (
    <span className="rounded-full border bg-muted px-2.5 py-1 text-xs font-medium text-muted-foreground">
      In progress
    </span>
  );

  return (
    <article className="min-h-full w-full">
      <PresentationContextBar
        variant="document"
        contextType="Participation"
        contextLabel={title}
        navigation={
          <DocumentTableOfContents location={location} />
        }
        actions={status}
      />

      <div className="mx-auto w-full max-w-4xl px-8 py-6">
        <div className="space-y-8">
          {PARTICIPATION_SECTION_DEFINITIONS.map((definition) =>
          {
            const isCurrent = definition.value === location.localView;

            return (
              <section
                key={definition.value}
                id={definition.value}
                aria-labelledby={`${definition.value}-title`}
                data-document-section-status={
                  isCurrent ? "current" : "available"
                }
                className={
                  isCurrent
                    ? "scroll-mt-20 rounded-xl border border-primary/40 bg-card p-6 shadow-sm"
                    : "scroll-mt-20 rounded-xl border bg-card p-6"
                }
              >
                <h2
                  id={`${definition.value}-title`}
                  className="text-xl font-semibold"
                >
                  {definition.label}
                </h2>

                <p className="mt-3 text-sm leading-6 text-muted-foreground">
                  {SECTION_DESCRIPTIONS[definition.value]}
                </p>

                <div className="mt-6 h-24 rounded-lg border border-dashed bg-muted/30" />
              </section>
            );
          })}
        </div>
      </div>
    </article>
  );
}
