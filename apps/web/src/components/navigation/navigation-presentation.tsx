import { DocumentPresentation } from "@/components/navigation/document-presentation";
import { ExplorerPresentation } from "@/components/navigation/explorer-presentation";

import
{
  NavigationScrollRestoration,
} from "@/components/navigation/navigation-scroll-restoration";

import
{
  NavigationStateRecorder,
} from "@/components/navigation/navigation-state-recorder";

import { PersonalHomePresentation } from "@/components/navigation/personal-home-presentation";
import { WorkspacePresentation } from "@/components/navigation/workspace-presentation";

import type
{
  NavigationLocation,
} from "@/navigation/navigation-types";

type NavigationPresentationProps =
{
  location: NavigationLocation;
};

function renderPresentation(
  location: NavigationLocation
): React.ReactNode
{
  switch (location.contextKind)
  {
    case "home":
    {
      return (
        <PersonalHomePresentation />
      );
    }

    case "organization":
    case "project":
    {
      return (
        <ExplorerPresentation
          location={location}
        />
      );
    }

    case "campaign":
    {
      return (
        <WorkspacePresentation
          location={location}
        />
      );
    }

    case "participation":
    {
      return (
        <DocumentPresentation
          location={location}
        />
      );
    }
  }
}

export function NavigationPresentation(
  {
    location,
  }: Readonly<NavigationPresentationProps>
)
{
  return (
    <>
      <NavigationStateRecorder
        location={location}
      />

      <NavigationScrollRestoration
        location={location}
      />

      {renderPresentation(location)}
    </>
  );
}
