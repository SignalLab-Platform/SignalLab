"use client";

import { useMemo, useState } from "react";
import { Building2 } from "lucide-react";

import
{
  ContextBreadcrumb,
} from "@/components/navigation/context-breadcrumb";

import
{
  LocalNavigation,
} from "@/components/navigation/local-navigation";

import
{
  OrganizationMembersActions,
  OrganizationMembersContent,
} from "@/components/navigation/organization-members-presentation";

import
{
  OrganizationInvitationsActions,
  OrganizationInvitationsContent,
} from "@/components/navigation/organization-invitations-presentation";

import
{
  OrganizationSettingsContent,
} from "@/components/navigation/organization-settings-presentation";

import
{
  useOrganizationRoles,
} from "@/features/organizations/use-organization-roles";

import type
{
  MemberStatusFilter,
} from "@/components/navigation/organization-members-presentation";

import
{
  PresentationContextBar,
} from "@/components/navigation/presentation-context-bar";

import
{
  useOrganizationMembers,
} from "@/features/organizations/use-organization-members";

import
{
  useOrganizations,
} from "@/features/organizations/use-organizations";

import
{
  buildOrganizationRoute,
} from "@/navigation/navigation-routes";

import
{
  getPersistedLocalView,
  useNavigationState,
} from "@/navigation/navigation-state";

import
{
  ORGANIZATION_VIEWS,
} from "@/navigation/navigation-types";

import type
{
  OrganizationAdministrationNavigationLocation,
} from "@/navigation/navigation-types";

type OrganizationAdministrationPresentationProps =
{
  location: OrganizationAdministrationNavigationLocation;
};

export function OrganizationAdministrationPresentation(
  { location }: Readonly<OrganizationAdministrationPresentationProps>
)
{
  const navigationState = useNavigationState();

  const [searchQuery, setSearchQuery] = useState("");
  const [roleFilter, setRoleFilter] = useState("all");
  const [statusFilter, setStatusFilter] =
    useState<MemberStatusFilter>("active");

  const explorerView = getPersistedLocalView(
    navigationState,
    `organization:${location.organizationId}`,
    ORGANIZATION_VIEWS,
    "all"
  );

  const explorerHref = buildOrganizationRoute(
    location.organizationId,
    explorerView
  );

  const organizationsQuery = useOrganizations();

  const organization = organizationsQuery.data?.find((item) =>
  {
    return item.id === location.organizationId;
  });

  const organizationName = organization?.name
    ?? (organizationsQuery.isPending
      ? "Loading..."
      : "Organization unavailable");

  const membersQuery = useOrganizationMembers(
    location.organizationId
  );

  const members = membersQuery.data;

  const roles = useMemo(() =>
  {
    if (!members)
    {
      return [];
    }

    return Array.from(
      new Set(members.map((member) => member.roleName))
    ).sort();
  }, [members]);

  const rolesQuery = useOrganizationRoles(
    location.organizationId,
    location.localView === "settings"
  );

  const breadcrumbItems =
  [
    {
      label: organizationName,
      href: explorerHref,
      icon: Building2,
    },
    {
      label: "Administration",
    },
  ];

  const actions = location.localView === "members"
    ? (
        <OrganizationMembersActions
        searchQuery={searchQuery}
        onSearchQueryChange={setSearchQuery}
        roleFilter={roleFilter}
        onRoleFilterChange={setRoleFilter}
        statusFilter={statusFilter}
        onStatusFilterChange={setStatusFilter}
        roles={roles}
        />
    )
    : location.localView === "invitations"
        ? <OrganizationInvitationsActions />
        : undefined;

  return (
    <section className="min-h-full w-full">
      <PresentationContextBar
        variant="explorer"
        breadcrumb={
          <ContextBreadcrumb
            items={breadcrumbItems}
            backHref={explorerHref}
          />
        }
        navigation={<LocalNavigation location={location} />}
        actions={actions}
      />

      <div className="mx-auto w-full max-w-6xl px-8 py-6">
        {location.localView === "members"
          ? (
            <OrganizationMembersContent
              members={members ?? []}
              searchQuery={searchQuery}
              roleFilter={roleFilter}
              statusFilter={statusFilter}
              isPending={membersQuery.isPending}
              isError={membersQuery.isError}
              onRetry={() =>
              {
                void membersQuery.refetch();
              }}
            />
          )
          : null}

        {location.localView === "invitations"
          ? <OrganizationInvitationsContent />
          : null}

        {location.localView === "settings"
          ? (
            <OrganizationSettingsContent
              organization={organization}
              isPending={organizationsQuery.isPending}
              isError={organizationsQuery.isError}
              onRetry={() =>
              {
                void organizationsQuery.refetch();
              }}
              roles={rolesQuery.data ?? []}
              rolesIsPending={rolesQuery.isPending}
              rolesIsError={rolesQuery.isError}
              onRetryRoles={() =>
              {
                void rolesQuery.refetch();
              }}
            />
          )
        : null}
      </div>
    </section>
  );
}