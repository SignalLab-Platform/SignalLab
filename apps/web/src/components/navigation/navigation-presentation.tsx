import { DocumentPresentation } from "@/components/navigation/document-presentation";
import { ObjectExplorerPresentation } from "@/components/navigation/object-explorer-presentation";
import { OrganizationAdministrationPresentation } from "@/components/navigation/organization-administration-presentation";
import { PersonalHomePresentation } from "@/components/navigation/personal-home-presentation";
import { WorkspacePresentation } from "@/components/navigation/workspace-presentation";

import
{
  NavigationScrollRestoration,
} from "@/components/navigation/navigation-scroll-restoration";

import
{
  NavigationStateRecorder,
} from "@/components/navigation/navigation-state-recorder";

import type
{
  NavigationLocation,
} from "@/navigation/navigation-types";

import {
  OrganizationInvitationPresentation,
} from "@/features/organization-invitations/organization-invitation-presentation";

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

    case "organization-invitation":
    {
      return (
        <OrganizationInvitationPresentation
          invitationId={location.invitationId}
        />
      );
    }

    case "organization":
    case "project":
    {
      return (
        <ObjectExplorerPresentation location={location} />
      );
    }

    case "organization-administration":
    {
      return (
        <OrganizationAdministrationPresentation location={location} />
      );
    }

    case "campaign":
    {
      return (
        <WorkspacePresentation location={location} />
      );
    }

    case "participation":
    {
      return (
        <DocumentPresentation location={location} />
      );
    }
  }
}

export function NavigationPresentation(
  { location }: Readonly<NavigationPresentationProps>
)
{
  return (
    <>
      <NavigationStateRecorder location={location} />

      <NavigationScrollRestoration location={location} />

      {renderPresentation(location)}
    </>
  );
}