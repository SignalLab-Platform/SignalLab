import Link from "next/link";

import { buildParticipationRoute } from "@/navigation/navigation-routes";

import
{
  PARTICIPATION_SECTION_DEFINITIONS,
} from "@/navigation/navigation-view-definitions";

import type
{
  ParticipationNavigationLocation,
} from "@/navigation/navigation-types";

type DocumentTableOfContentsProps =
{
  location: ParticipationNavigationLocation;
  className?: string;
};

function getLinkClassName(isCurrent: boolean): string
{
  const baseClassName =
    "inline-flex h-9 items-center rounded-lg px-3 text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring";

  if (isCurrent)
  {
    return `${baseClassName} bg-primary/12 text-foreground`;
  }

  return `${baseClassName} text-muted-foreground hover:bg-accent hover:text-accent-foreground`;
}

export function DocumentTableOfContents(
  {
    location,
    className,
  }: Readonly<DocumentTableOfContentsProps>
)
{
  return (
    <nav
      aria-label="Table of contents"
      className={`min-w-0 overflow-x-auto ${className ?? ""}`}
    >
      <div className="flex min-w-max items-center gap-1">
        {PARTICIPATION_SECTION_DEFINITIONS.map((definition) =>
        {
          const isCurrent = definition.value === location.localView;

          const route = buildParticipationRoute(
            location.participationId,
            definition.value
          );

          return (
            <Link
              key={definition.value}
              href={`${route}#${definition.value}`}
              scroll={false}
              aria-current={isCurrent ? "location" : undefined}
              className={getLinkClassName(isCurrent)}
            >
              {definition.label}
            </Link>
          );
        })}
      </div>
    </nav>
  );
}
