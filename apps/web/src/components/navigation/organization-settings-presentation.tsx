"use client";

import { Building2, Pencil, Trash2 } from "lucide-react";

import { Button } from "@/components/ui/button";

import type
{
  Organization,
} from "@/features/organizations/organization";

import
{
  OrganizationRolesContent,
} from "@/components/navigation/organization-roles-presentation";

import type
{
  OrganizationRole,
} from "@/features/organizations/organization-role";

type OrganizationSettingsContentProps =
{
  organization: Organization | undefined;
  isPending: boolean;
  isError: boolean;
  onRetry: () => void;

  roles: OrganizationRole[];
  rolesIsPending: boolean;
  rolesIsError: boolean;
  onRetryRoles: () => void;
};

type OrganizationSettingsFieldProps =
{
  label: string;
  value?: string | null;
  unavailable?: boolean;
};

function OrganizationSettingsField(
  {
    label,
    value,
    unavailable = false,
  }: Readonly<OrganizationSettingsFieldProps>
)
{
  return (
    <div className="flex items-start justify-between gap-6 py-5">
      <div className="min-w-0 flex-1 space-y-1">
        <p className="text-sm font-medium">
          {label}
        </p>

        <p className="wrap-break-word text-sm text-muted-foreground">
          {unavailable
            ? "Not available yet"
            : value || "Not provided"}
        </p>
      </div>

      <Button
        type="button"
        variant="outline"
        size="sm"
        disabled
        aria-label={`Edit ${label.toLowerCase()}`}
        title="Organization settings editing will be available in SL-034."
      >
        <Pencil aria-hidden="true" className="size-4" />
        Edit
      </Button>
    </div>
  );
}

function OrganizationSettingsLogo()
{
  return (
    <section>
      <div className="flex items-center gap-5">
        <div
          aria-hidden="true"
          className="flex size-20 shrink-0 items-center justify-center rounded-xl bg-muted"
        >
          <Building2 className="size-9 text-muted-foreground" />
        </div>

        <div className="min-w-0 flex-1 space-y-1">
          <h2 className="text-sm font-medium">
            Organization logo
          </h2>

          <p className="text-sm text-muted-foreground">
            Not available yet
          </p>
        </div>

        <Button
          type="button"
          variant="outline"
          size="sm"
          disabled
          title="Organization logo management will be available in SL-034."
        >
          Change
        </Button>
      </div>
    </section>
  );
}

function OrganizationSettingsDangerZone()
{
  return (
    <section
      aria-labelledby="organization-danger-zone-title"
      className="mt-8 border-t border-border pt-6"
    >
      <div className="space-y-1">
        <h2
          id="organization-danger-zone-title"
          className="text-base font-semibold text-destructive"
        >
          Danger zone
        </h2>

        <p className="text-sm text-muted-foreground">
          Permanently delete this organization and its associated resources.
        </p>
      </div>

      <Button
        type="button"
        variant="outline"
        disabled
        className="mt-5 text-destructive"
        title="Organization deletion is not available yet."
      >
        <Trash2 aria-hidden="true" className="size-4" />
        Delete organization
      </Button>
    </section>
  );
}

export function OrganizationSettingsContent(
  {
    organization,
    isPending,
    isError,
    onRetry,
    roles,
    rolesIsPending,
    rolesIsError,
    onRetryRoles,
  }: Readonly<OrganizationSettingsContentProps>
)
{
  if (isPending)
  {
    return (
      <div
        role="status"
        className="text-sm text-muted-foreground"
      >
        Loading organization settings...
      </div>
    );
  }

  if (isError)
  {
    return (
      <div role="alert" className="space-y-3">
        <p className="text-sm text-destructive">
          Unable to load organization settings.
        </p>

        <Button
          type="button"
          variant="outline"
          onClick={onRetry}
        >
          Retry
        </Button>
      </div>
    );
  }

  if (!organization)
  {
    return (
      <div role="status" className="text-sm text-muted-foreground">
        Organization unavailable.
      </div>
    );
  }

  return (
    <div className="w-full">
      <OrganizationSettingsLogo />

      <section
        aria-labelledby="organization-general-information-title"
        className="mt-8 border-t border-border pt-6"
      >
        <div className="space-y-1">
          <h2
            id="organization-general-information-title"
            className="text-base font-semibold"
          >
            General information
          </h2>

          <p className="text-sm text-muted-foreground">
            Information associated with this organization.
          </p>
        </div>

        <div className="mt-6 divide-y divide-border">
          <OrganizationSettingsField
            label="Name"
            value={organization.name}
          />

          <OrganizationSettingsField
            label="Country"
            value={organization.country}
          />

          <OrganizationSettingsField
            label="Description"
            unavailable
          />

          <OrganizationSettingsField
            label="Website"
            value={organization.website}
          />
        </div>
      </section>

      <OrganizationRolesContent
        roles={roles}
        isPending={rolesIsPending}
        isError={rolesIsError}
        onRetry={onRetryRoles}
      />

      <OrganizationSettingsDangerZone />
    </div>
  );
}