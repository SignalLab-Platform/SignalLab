import { fireEvent, render, screen } from "@testing-library/react";

import
{
  OrganizationRolesContent,
} from "@/components/navigation/organization-roles-presentation";

import type
{
  OrganizationRole,
} from "@/features/organizations/organization-role";

const roles: OrganizationRole[] =
[
  {
    id: "role-owner",
    name: "Owner",
    description: "Organization owner",
    isOwner: true,
    permissions: ["ManageOrganization", "ManageMembers"],
    createdAt: "2026-01-01T00:00:00Z",
    updatedAt: "2026-01-01T00:00:00Z",
  },
  {
    id: "role-member",
    name: "Member",
    description: null,
    isOwner: false,
    permissions: ["ViewAnalytics"],
    createdAt: "2026-01-01T00:00:00Z",
    updatedAt: "2026-01-01T00:00:00Z",
  },
];

function renderRoles(
  overrides: Partial<React.ComponentProps<typeof OrganizationRolesContent>> = {}
)
{
  const onRetry = jest.fn();

  render(
    <OrganizationRolesContent
      roles={roles}
      isPending={false}
      isError={false}
      onRetry={onRetry}
      {...overrides}
    />
  );

  return { onRetry };
}

describe("OrganizationRolesContent", () =>
{
  it("displays roles returned by the API", () =>
  {
    renderRoles();

    expect(screen.getAllByText("Owner")).toHaveLength(2);
    expect(screen.getByText("Member")).toBeInTheDocument();
    expect(screen.getByText("Organization owner")).toBeInTheDocument();
  });

  it("displays permissions using readable labels", () =>
  {
    renderRoles();

    expect(screen.getByText("Manage Organization")).toBeInTheDocument();
    expect(screen.getByText("Manage Members")).toBeInTheDocument();
    expect(screen.getByText("View Analytics")).toBeInTheDocument();
  });

  it("allows expanding a role", () =>
  {
    renderRoles();

    const ownerSummary = screen.getByText("Organization owner")
      .closest("summary");

    expect(ownerSummary).not.toBeNull();

    const details = ownerSummary!.closest("details");

    expect(details).not.toHaveAttribute("open");

    fireEvent.click(ownerSummary!);

    expect(details).toHaveAttribute("open");
  });

  it("displays an actual empty state", () =>
  {
    renderRoles({ roles: [] });

    expect(screen.getByRole("status")).toHaveTextContent(
      "No roles found for this organization."
    );
  });

  it("distinguishes loading from an empty list", () =>
  {
    renderRoles(
    {
      roles: [],
      isPending: true,
    });

    expect(screen.getByRole("status")).toHaveTextContent(
      "Loading organization roles..."
    );
  });

  it("displays an error and supports retry", () =>
  {
    const { onRetry } = renderRoles(
    {
      roles: [],
      isError: true,
    });

    expect(screen.getByRole("alert")).toHaveTextContent(
      "Unable to load organization roles."
    );

    fireEvent.click(
      screen.getByRole("button",
      {
        name: "Retry roles",
      })
    );

    expect(onRetry).toHaveBeenCalledTimes(1);
  });
});