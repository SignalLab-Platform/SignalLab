import
{
  fireEvent,
  render,
  screen,
  waitFor,
} from "@testing-library/react";

import
{
  buildOrganizationAdministrationRoute,
} from "@/navigation/navigation-routes";

import
{
  resolveNavigationLocation,
} from "@/navigation/navigation-resolver";

import type
{
  OrganizationAdministrationView,
} from "@/navigation/navigation-types";

import
{
  OrganizationAdministrationPresentation,
} from "@/components/navigation/organization-administration-presentation";

import
{
  useOrganizationMembers,
} from "@/features/organizations/use-organization-members";

import
{
  useOrganizationRoles,
} from "@/features/organizations/use-organization-roles";

import type
{
  OrganizationRole,
} from "@/features/organizations/organization-role";

import
{
  useOrganizations,
} from "@/features/organizations/use-organizations";

import type
{
  OrganizationMember,
} from "@/features/organizations/organization-member";

import {
  useOrganizationInvitations,
} from "@/features/organization-invitations/use-organization-invitations";

import {
  useCancelOrganizationInvitation,
} from "@/features/organization-invitations/use-cancel-organization-invitation";

import { useCurrentUser } from "@/features/current-user/use-current-user";

import
{
  resetNavigationState,
} from "@/navigation/navigation-state";

import type
{
  OrganizationAdministrationNavigationLocation,
} from "@/navigation/navigation-types";

jest.mock("@/features/organizations/use-organizations");
jest.mock("@/features/organizations/use-organization-members");
jest.mock("@/features/organizations/use-organization-roles");
jest.mock("@/features/organization-invitations/use-organization-invitations");
jest.mock("@/features/current-user/use-current-user");

jest.mock(
  "@/features/organization-invitations/create-organization-invitation-dialog",
  () =>
  ({
    CreateOrganizationInvitationDialog: () => null,
  })
);

jest.mock(
  "@/features/organization-invitations/use-cancel-organization-invitation"
);

Object.defineProperty(HTMLElement.prototype, "scrollIntoView",
{
  configurable: true,
  writable: true,
  value: jest.fn(),
});

describe("OrganizationAdministrationPresentation", () =>
{
  const mockedUseOrganizations = jest.mocked(useOrganizations);
  const mockedUseOrganizationMembers = jest.mocked(useOrganizationMembers);
  const mockedUseOrganizationInvitations = jest.mocked(useOrganizationInvitations);

  const alphaMembers: OrganizationMember[] =
  [
    {
      id: "membership-alpha-one",
      userId: "user-alpha-one",
      firstName: "Alice",
      lastName: "Martin",
      email: "alice@example.com",
      roleId: "role-owner",
      roleName: "Owner",
      status: "Active",
      joinedAt: "2026-01-15T10:00:00Z",
      removedAt: null,
    },
    {
      id: "membership-alpha-two",
      userId: "user-alpha-two",
      firstName: "Bob",
      lastName: "Durand",
      email: "bob@example.com",
      roleId: "role-member",
      roleName: "Member",
      status: "Active",
      joinedAt: "2026-02-20T10:00:00Z",
      removedAt: null,
    },
    {
      id: "membership-alpha-three",
      userId: "user-alpha-three",
      firstName: "Claire",
      lastName: "Bernard",
      email: "claire@example.com",
      roleId: "role-member",
      roleName: "Member",
      status: "Removed",
      joinedAt: "2026-03-01T10:00:00Z",
      removedAt: "2026-04-01T10:00:00Z",
    },
  ];

  const betaMembers: OrganizationMember[] =
  [
    {
      id: "membership-beta-one",
      userId: "user-beta-one",
      firstName: "David",
      lastName: "Moreau",
      email: "david@example.com",
      roleId: "role-owner",
      roleName: "Owner",
      status: "Active",
      joinedAt: "2026-05-01T10:00:00Z",
      removedAt: null,
    },
  ];

  const refetchMembers = jest.fn();

  const mockedUseOrganizationRoles = jest.mocked(useOrganizationRoles);

  const alphaRoles: OrganizationRole[] =
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
      description: "Standard organization member",
      isOwner: false,
      permissions: ["ViewAnalytics"],
      createdAt: "2026-01-01T00:00:00Z",
      updatedAt: "2026-01-01T00:00:00Z",
    },
  ];

  const betaRoles: OrganizationRole[] =
  [
    {
      id: "role-beta-owner",
      name: "Beta Owner",
      description: null,
      isOwner: true,
      permissions: ["ManageOrganization"],
      createdAt: "2026-01-01T00:00:00Z",
      updatedAt: "2026-01-01T00:00:00Z",
    },
  ];

  const refetchRoles = jest.fn();

  function createLocation(
    organizationId: string,
    localView: OrganizationAdministrationView = "members"
  ): OrganizationAdministrationNavigationLocation
  {
    const pathname = buildOrganizationAdministrationRoute(
      organizationId,
      localView
    );

    const location = resolveNavigationLocation(pathname);

    if (location?.contextKind !== "organization-administration")
    {
      throw new Error(
        `Unable to resolve Organization Administration route: ${pathname}`
      );
    }

    return location;
  }

  function mockMembers(
    data: OrganizationMember[] | undefined,
    isPending = false,
    isError = false
  )
  {
    mockedUseOrganizationMembers.mockReturnValue(
    {
      data,
      isPending,
      isError,
      refetch: refetchMembers,
    } as unknown as ReturnType<typeof useOrganizationMembers>);
  }

  beforeEach(() =>
  {
    jest.clearAllMocks();
    resetNavigationState();

    mockedUseOrganizations.mockReturnValue(
    {
      data:
      [
        {
          id: "organization-alpha",
          name: "Studio Alpha",
          country: "FR",
          website: null,
        },
        {
          id: "organization-beta",
          name: "Studio Beta",
          country: "FR",
          website: null,
        },
      ],
      isPending: false,
      isError: false,
    } as ReturnType<typeof useOrganizations>);

    mockedUseOrganizationMembers.mockImplementation((organizationId) =>
    {
      return {
        data: organizationId === "organization-alpha"
          ? alphaMembers
          : betaMembers,
        isPending: false,
        isError: false,
        refetch: refetchMembers,
      } as unknown as ReturnType<typeof useOrganizationMembers>;
    });
  });

  mockedUseOrganizationRoles.mockImplementation((organizationId) =>
  {
    return {
      data: organizationId === "organization-alpha"
      ? alphaRoles
      : betaRoles,
      isPending: false,
      isError: false,
      refetch: refetchRoles,
    } as unknown as ReturnType<typeof useOrganizationRoles>;
  });

  mockedUseOrganizationInvitations.mockReturnValue(
  {
    data: [],
    isPending: false,
    isError: false,
    refetch: jest.fn(),
  } as unknown as ReturnType<typeof useOrganizationInvitations>);

  jest.mocked(useCurrentUser).mockReturnValue(
  {
    data:
    {
      id: "user-alpha-one",
      email: "alice@example.com",
    },
    isPending: false,
    isError: false,
  } as unknown as ReturnType<typeof useCurrentUser>);

  jest.mocked(useCancelOrganizationInvitation).mockReturnValue(
  {
    mutateAsync: jest.fn(),
    isPending: false,
    isError: false,
    error: null,
  } as unknown as ReturnType<typeof useCancelOrganizationInvitation>);

  it("renders the Organization breadcrumb and Members navigation", () =>
  {
    render(
      <OrganizationAdministrationPresentation
        location={createLocation("organization-alpha")}
      />
    );

    expect(
      screen.getByRole("navigation",
      {
        name: "Context breadcrumb",
      })
    ).toHaveTextContent("Studio Alpha");

    expect(
      screen.getByRole("link",
      {
        name: "Go to parent context",
      })
    ).toHaveAttribute(
      "href",
      "/organizations/organization-alpha/all"
    );

    expect(
      screen.getByRole("link",
      {
        name: "Members",
      })
    ).toHaveAttribute("aria-current", "page");

    expect(screen.getByText("Alice Martin")).toBeInTheDocument();
    expect(screen.getByText("Bob Durand")).toBeInTheDocument();

    expect(
      screen.queryByText("Claire Bernard")
    ).not.toBeInTheDocument();
  });

  it("updates the members table when searching from the header", () =>
  {
    render(
      <OrganizationAdministrationPresentation
        location={createLocation("organization-alpha")}
      />
    );

    fireEvent.change(
      screen.getByRole("searchbox",
      {
        name: "Search members",
      }),
      {
        target:
        {
          value: "BOB",
        },
      }
    );

    expect(screen.getByText("Bob Durand")).toBeInTheDocument();

    expect(
      screen.queryByText("Alice Martin")
    ).not.toBeInTheDocument();

    expect(screen.getByText("1 of 3 members")).toBeInTheDocument();
  });

  it("filters the table by role using the header Select", async () =>
  {
    render(
      <OrganizationAdministrationPresentation
        location={createLocation("organization-alpha")}
      />
    );

    const roleFilter = screen.getByRole("combobox",
    {
      name: "Filter members by role",
    });

    fireEvent.click(roleFilter);

    const ownerOption = await screen.findByRole("option",
    {
      name: "Owner",
    });

    fireEvent.click(ownerOption);

    await waitFor(() =>
    {
      expect(screen.getByText("Alice Martin")).toBeInTheDocument();

      expect(
        screen.queryByText("Bob Durand")
      ).not.toBeInTheDocument();
    });

    expect(screen.getByText("1 of 3 members")).toBeInTheDocument();
  });

  it("filters removed members using the header Select", async () =>
  {
    render(
      <OrganizationAdministrationPresentation
        location={createLocation("organization-alpha")}
      />
    );

    const statusFilter = screen.getByRole("combobox",
    {
      name: "Filter members by status",
    });

    fireEvent.click(statusFilter);

    const removedOption = await screen.findByRole("option",
    {
      name: "Removed",
    });

    fireEvent.click(removedOption);

    await waitFor(() =>
    {
      expect(screen.getByText("Claire Bernard")).toBeInTheDocument();

      expect(
        screen.queryByText("Alice Martin")
      ).not.toBeInTheDocument();
    });

    expect(screen.getByText("1 of 3 members")).toBeInTheDocument();
  });

  it("combines search and role filters", async () =>
  {
    render(
      <OrganizationAdministrationPresentation
        location={createLocation("organization-alpha")}
      />
    );

    fireEvent.change(
      screen.getByRole("searchbox",
      {
        name: "Search members",
      }),
      {
        target:
        {
          value: "bob",
        },
      }
    );

    fireEvent.click(
      screen.getByRole("combobox",
      {
        name: "Filter members by role",
      })
    );

    fireEvent.click(
      await screen.findByRole("option",
      {
        name: "Owner",
      })
    );

    await waitFor(() =>
    {
      expect(screen.getByText("No members found")).toBeInTheDocument();
    });

    expect(screen.getByText("0 of 3 members")).toBeInTheDocument();
  });

  it("loads members belonging to the selected Organization", () =>
  {
    const { rerender } = render(
      <OrganizationAdministrationPresentation
        location={createLocation("organization-alpha")}
      />
    );

    expect(screen.getByText("Alice Martin")).toBeInTheDocument();

    rerender(
      <OrganizationAdministrationPresentation
        location={createLocation("organization-beta")}
      />
    );

    expect(mockedUseOrganizationMembers).toHaveBeenCalledWith(
      "organization-beta"
    );

    expect(screen.getByText("David Moreau")).toBeInTheDocument();

    expect(
      screen.queryByText("Alice Martin")
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole("navigation",
      {
        name: "Context breadcrumb",
      })
    ).toHaveTextContent("Studio Beta");
  });

  it("hides Members actions on the Invitations tab", () =>
  {
    render(
      <OrganizationAdministrationPresentation
        location={createLocation("organization-alpha", "invitations")}
      />
    );

    expect(screen.getByText("No invitations")).toBeInTheDocument();

    expect(mockedUseOrganizationInvitations).toHaveBeenCalledWith(
      "organization-alpha",
      true
    );

    expect(screen.queryByRole("table")).not.toBeInTheDocument();

    expect(
      screen.getByRole("button",
      {
        name: "Invite member",
      })
    ).toBeEnabled();

    expect(
      screen.queryByRole("searchbox",
      {
        name: "Search members",
      })
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole("columnheader",
      {
        name: "Member",
      })
    ).not.toBeInTheDocument();
  });

  it("hides Members actions on the Settings tab", () =>
  {
    render(
      <OrganizationAdministrationPresentation
        location={createLocation("organization-alpha", "settings")}
      />
    );

    expect(
      screen.getByRole("heading",
      {
        name: "General information",
      })
    ).toBeInTheDocument();

    expect(
      screen.getByRole("button",
      {
        name: "Edit name",
      })
    ).toBeDisabled();

    expect(
      screen.getByRole("button",
      {
        name: "Edit country",
      })
    ).toBeDisabled();

    expect(
      screen.getByRole("button",
      {
        name: "Edit description",
      })
    ).toBeDisabled();

    expect(
      screen.getByRole("button",
      {
        name: "Edit website",
      })
    ).toBeDisabled();

    expect(
      screen.getByRole("heading",
      {
        name: "Roles & Permissions",
      })
    ).toBeInTheDocument();

    expect(
      screen.getByText("Standard organization member")
    ).toBeInTheDocument();

    expect(
      screen.getByText("Manage Organization")
    ).toBeInTheDocument();

    expect(
      screen.getByText("View Analytics")
    ).toBeInTheDocument();

    expect(
      screen.getByRole("button",
      {
        name: "Delete organization",
      })
    ).toBeDisabled();

    expect(
      screen.queryByRole("button",
      {
        name: "Invite member",
      })
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole("table")
    ).not.toBeInTheDocument();
  });

  it("loads roles for all administration views", () =>
  {
    const { rerender } = render(
      <OrganizationAdministrationPresentation
        location={createLocation("organization-alpha", "members")}
      />
    );

    expect(mockedUseOrganizationRoles).toHaveBeenLastCalledWith(
      "organization-alpha",
      true
    );

    rerender(
      <OrganizationAdministrationPresentation
        location={createLocation("organization-alpha", "invitations")}
      />
    );

    expect(mockedUseOrganizationRoles).toHaveBeenLastCalledWith(
      "organization-alpha",
      true
    );

    rerender(
      <OrganizationAdministrationPresentation
        location={createLocation("organization-alpha", "settings")}
      />
    );

    expect(mockedUseOrganizationRoles).toHaveBeenLastCalledWith(
      "organization-alpha",
      true
    );
  });

  it("displays roles belonging to the selected Organization", () =>
  {
    const { rerender } = render(
      <OrganizationAdministrationPresentation
        location={createLocation("organization-alpha", "settings")}
      />
    );

    expect(
      screen.getByText("Standard organization member")
    ).toBeInTheDocument();

    rerender(
      <OrganizationAdministrationPresentation
        location={createLocation("organization-beta", "settings")}
      />
    );

    expect(mockedUseOrganizationRoles).toHaveBeenLastCalledWith(
      "organization-beta",
      true
    );

    expect(
      screen.getByText("Beta Owner")
    ).toBeInTheDocument();

    expect(
      screen.queryByText("Standard organization member")
    ).not.toBeInTheDocument();
  });

  it("displays an API error and retries the request", () =>
  {
    mockMembers(undefined, false, true);

    render(
      <OrganizationAdministrationPresentation
        location={createLocation("organization-alpha")}
      />
    );

    expect(screen.getByRole("alert")).toHaveTextContent(
      "Unable to load organization members."
    );

    fireEvent.click(
      screen.getByRole("button",
      {
        name: "Retry",
      })
    );

    expect(refetchMembers).toHaveBeenCalledTimes(1);
  });

  it("displays the loading state", () =>
  {
    mockMembers(undefined, true, false);

    render(
      <OrganizationAdministrationPresentation
        location={createLocation("organization-alpha")}
      />
    );

    expect(screen.getByRole("status")).toHaveTextContent(
      "Loading organization members..."
    );
  });

  it("hides invitation actions without ManageMembers permission", () =>
  {
    jest.mocked(useCurrentUser).mockReturnValue(
    {
      data:
      {
        id: "user-alpha-two",
        email: "bob@example.com",
      },
      isPending: false,
      isError: false,
    } as unknown as ReturnType<typeof useCurrentUser>);

    render(
      <OrganizationAdministrationPresentation
        location={createLocation("organization-alpha", "invitations")}
      />
    );

    expect(
      screen.queryByRole("button", { name: "Invite member" })
    ).not.toBeInTheDocument();
  });
});