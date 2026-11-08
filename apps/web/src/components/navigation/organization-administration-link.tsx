"use client";

import Link from "next/link";
import { Settings } from "lucide-react";

import
{
  getPersistedLocalView,
  useNavigationState,
} from "@/navigation/navigation-state";

import
{
  buildOrganizationAdministrationRoute,
} from "@/navigation/navigation-routes";

import
{
  ORGANIZATION_ADMINISTRATION_VIEWS,
} from "@/navigation/navigation-types";

type OrganizationAdministrationLinkProps =
{
  organizationId: string;
  organizationName: string;
};

export function OrganizationAdministrationLink(
  {
    organizationId,
    organizationName,
  }: Readonly<OrganizationAdministrationLinkProps>
)
{
  const navigationState = useNavigationState();

  const administrationView = getPersistedLocalView(
    navigationState,
    `organization-administration:${organizationId}`,
    ORGANIZATION_ADMINISTRATION_VIEWS,
    "members"
  );

  const administrationHref = buildOrganizationAdministrationRoute(
    organizationId,
    administrationView
  );

  return (
    <Link
      href={administrationHref}
      scroll={false}
      aria-label={`Manage ${organizationName}`}
      title={`Manage ${organizationName}`}
      className="inline-flex size-8 items-center justify-center rounded-md text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
    >
      <Settings
        aria-hidden="true"
        className="size-4"
      />
    </Link>
  );
}