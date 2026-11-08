import { fireEvent, render, screen, within } from "@testing-library/react";

import { userEvent } from "@testing-library/user-event";

import
{
  OrganizationMembersContent,
} from "@/components/navigation/organization-members-presentation";

import type
{
  OrganizationMember,
} from "@/features/organizations/organization-member";

Object.defineProperty(HTMLElement.prototype, "hasPointerCapture",
{
  configurable: true,
  value: jest.fn().mockReturnValue(false),
});

Object.defineProperty(HTMLElement.prototype, "setPointerCapture",
{
  configurable: true,
  value: jest.fn(),
});

Object.defineProperty(HTMLElement.prototype, "releasePointerCapture",
{
  configurable: true,
  value: jest.fn(),
});

Object.defineProperty(HTMLElement.prototype, "scrollIntoView",
{
  configurable: true,
  value: jest.fn(),
});

describe("OrganizationMembersContent", () =>
{
  const members: OrganizationMember[] =
  [
    {
      id: "membership-one",
      userId: "user-one",
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
      id: "membership-two",
      userId: "user-two",
      firstName: "Bob",
      lastName: null,
      email: "bob@example.com",
      roleId: "role-member",
      roleName: "Member",
      status: "Active",
      joinedAt: "2026-02-20T10:00:00Z",
      removedAt: null,
    },
    {
      id: "membership-three",
      userId: "user-three",
      firstName: null,
      lastName: null,
      email: "anonymous@example.com",
      roleId: "role-member",
      roleName: "Member",
      status: "Removed",
      joinedAt: "2026-03-01T10:00:00Z",
      removedAt: "2026-04-01T10:00:00Z",
    },
  ];

  const onRetry = jest.fn();

  function renderMembers(
    options: Partial<React.ComponentProps<
      typeof OrganizationMembersContent
    >> = {}
  )
  {
    return render(
      <OrganizationMembersContent
        members={members}
        searchQuery=""
        roleFilter="all"
        statusFilter="active"
        isPending={false}
        isError={false}
        onRetry={onRetry}
        {...options}
      />
    );
  }

  beforeEach(() =>
  {
    jest.clearAllMocks();
  });

  it("displays active members with their names and emails", () =>
  {
    renderMembers();

    expect(screen.getByText("Alice Martin")).toBeInTheDocument();
    expect(screen.getByText("alice@example.com")).toBeInTheDocument();
    expect(screen.getByText("Bob")).toBeInTheDocument();
    expect(screen.getByText("bob@example.com")).toBeInTheDocument();

    expect(
      screen.queryByText("anonymous@example.com")
    ).not.toBeInTheDocument();

    expect(screen.getByText("2 of 3 members")).toBeInTheDocument();
  });

  it("uses Unnamed member when both names are absent", () =>
  {
    renderMembers(
    {
      statusFilter: "all",
    });

    expect(screen.getByText("Unnamed member")).toBeInTheDocument();
    expect(
      screen.getByText("anonymous@example.com")
    ).toBeInTheDocument();
  });

  it("searches by first name without case sensitivity", () =>
  {
    renderMembers(
    {
      searchQuery: "ALICE",
    });

    expect(screen.getByText("Alice Martin")).toBeInTheDocument();
    expect(screen.queryByText("Bob")).not.toBeInTheDocument();
  });

  it("searches by last name", () =>
  {
    renderMembers(
    {
      searchQuery: "martin",
    });

    expect(screen.getByText("Alice Martin")).toBeInTheDocument();
    expect(screen.queryByText("Bob")).not.toBeInTheDocument();
  });

  it("searches by email", () =>
  {
    renderMembers(
    {
      searchQuery: "bob@example.com",
    });

    expect(screen.getByText("Bob")).toBeInTheDocument();
    expect(screen.queryByText("Alice Martin")).not.toBeInTheDocument();
  });

  it("filters members by role", () =>
  {
    renderMembers(
    {
      roleFilter: "Owner",
    });

    expect(screen.getByText("Alice Martin")).toBeInTheDocument();
    expect(screen.queryByText("Bob")).not.toBeInTheDocument();
  });

  it("filters removed members", () =>
  {
    renderMembers(
    {
      statusFilter: "removed",
    });

    expect(screen.getByText("Unnamed member")).toBeInTheDocument();
    expect(screen.queryByText("Alice Martin")).not.toBeInTheDocument();
    expect(screen.queryByText("Bob")).not.toBeInTheDocument();
  });

  it("combines search, role and status filters", () =>
  {
    renderMembers(
    {
      searchQuery: "anonymous",
      roleFilter: "Member",
      statusFilter: "removed",
    });

    expect(
      screen.getByText("anonymous@example.com")
    ).toBeInTheDocument();

    expect(screen.getByText("1 of 3 members")).toBeInTheDocument();
  });

  it("shows an empty result when no member matches", () =>
  {
    renderMembers(
    {
      searchQuery: "nonexistent",
    });

    expect(screen.getByText("No members found")).toBeInTheDocument();
    expect(screen.getByText("0 of 3 members")).toBeInTheDocument();
  });

  it("displays a loading state", () =>
  {
    renderMembers(
    {
      isPending: true,
    });

    expect(screen.getByRole("status")).toHaveTextContent(
      "Loading organization members..."
    );

    expect(screen.queryByRole("table")).not.toBeInTheDocument();
  });

  it("displays an error and allows retrying", () =>
  {
    renderMembers(
    {
      isError: true,
    });

    expect(screen.getByRole("alert")).toHaveTextContent(
      "Unable to load organization members."
    );

    fireEvent.click(
      screen.getByRole("button",
      {
        name: "Retry",
      })
    );

    expect(onRetry).toHaveBeenCalledTimes(1);
  });

  it("displays the joined date", () =>
  {
    renderMembers(
    {
      roleFilter: "Owner",
    });

    const row = screen.getByRole("row",
    {
      name: /Alice Martin/,
    });

    expect(within(row).getByText("Jan 15, 2026")).toBeInTheDocument();
  });

  it("displays disabled management actions for an active member", async () =>
  {
    render(
      <OrganizationMembersContent
        members={members}
        searchQuery=""
        roleFilter="all"
        statusFilter="active"
        isPending={false}
        isError={false}
        onRetry={jest.fn()}
      />
    );

    const user = userEvent.setup();

    await user.click(
      screen.getByRole("button",
      {
        name: "Actions for Alice Martin",
      })
      );

    expect(
      await screen.findByRole("menuitem",
      {
        name: "Change role",
      })
    ).toHaveAttribute("data-disabled");

    expect(
      await screen.findByRole("menuitem",
      {
        name: "Remove member",
      })
    ).toHaveAttribute("data-disabled");
  });

  it("displays the reactivation action for a removed member", async () =>
  {
    render(
      <OrganizationMembersContent
        members={members}
        searchQuery=""
        roleFilter="all"
        statusFilter="removed"
        isPending={false}
        isError={false}
        onRetry={jest.fn()}
      />
    );

    const user = userEvent.setup();

    await user.click(
      screen.getByRole("button",
      {
        name: "Actions for Unnamed member",
      })
    );

    expect(
      await screen.findByRole("menuitem",
      {
        name: "Reactivate member",
      })
    ).toHaveAttribute("data-disabled");

    expect(
      screen.queryByRole("menuitem",
      {
        name: "Remove member",
      })
    ).not.toBeInTheDocument();
  });
});