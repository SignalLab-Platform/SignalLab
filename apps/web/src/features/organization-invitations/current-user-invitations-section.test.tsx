import { fireEvent, render, screen } from "@testing-library/react";

import {
  CurrentUserInvitationsSection,
} from "./current-user-invitations-section";

import {
  useCurrentUserInvitations,
} from "./use-current-user-invitations";

import type {
  CurrentUserInvitation,
} from "./organization-invitation";

jest.mock("./use-current-user-invitations");

const mockRefetch = jest.fn();

const pendingInvitation: CurrentUserInvitation =
{
  id: "invitation-1",
  organizationId: "organization-1",
  organizationName: "Organization Alpha",
  invitedByUserId: "user-1",
  roleId: "role-member",
  roleName: "Member",
  status: "Pending",
  createdAt: "2026-01-01T10:00:00Z",
  updatedAt: "2026-01-01T10:00:00Z",
  expiresAt: "2099-01-08T10:00:00Z",
  respondedAt: null,
};

function mockInvitations(
  invitations: CurrentUserInvitation[],
  overrides: Record<string, unknown> = {}
): void
{
  jest.mocked(useCurrentUserInvitations).mockReturnValue(
  {
    data: invitations,
    isPending: false,
    isError: false,
    refetch: mockRefetch,
    ...overrides,
  } as unknown as ReturnType<typeof useCurrentUserInvitations>);
}

describe("CurrentUserInvitationsSection", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();
    mockInvitations([pendingInvitation]);
  });

  it("expands automatically when an invitation is pending", () =>
  {
    render(<CurrentUserInvitationsSection />);

    expect(
      screen.getByRole("button", { name: "Collapse invitations" })
    ).toHaveAttribute("aria-expanded", "true");

    expect(screen.getByText("1 pending")).toBeInTheDocument();

    expect(
      screen.getByRole("link", { name: /Organization Alpha/ })
    ).toHaveAttribute(
      "href",
      "/organization-invitations/invitation-1?from=%2Fhome"
    );
  });

  it("stays collapsed by default without pending invitations", () =>
  {
    mockInvitations(
    [
      {
        ...pendingInvitation,
        status: "Accepted",
      },
    ]);

    render(<CurrentUserInvitationsSection />);

    expect(
      screen.getByRole("button", { name: "Expand invitations" })
    ).toHaveAttribute("aria-expanded", "false");

    expect(screen.queryByText("Organization Alpha"))
      .not.toBeInTheDocument();

    fireEvent.click(
      screen.getByRole("button", { name: "Expand invitations" })
    );

    expect(screen.getByText("Organization Alpha"))
      .toBeInTheDocument();

    expect(screen.getByText("Accepted"))
      .toBeInTheDocument();
  });

  it("keeps the user's explicit collapse choice", () =>
  {
    const { rerender } = render(
      <CurrentUserInvitationsSection />
    );

    fireEvent.click(
      screen.getByRole("button", { name: "Collapse invitations" })
    );

    rerender(<CurrentUserInvitationsSection />);

    expect(
      screen.getByRole("button", { name: "Expand invitations" })
    ).toHaveAttribute("aria-expanded", "false");
  });

  it("does not count expired invitations as pending", () =>
  {
    mockInvitations(
    [
      {
        ...pendingInvitation,
        expiresAt: "2020-01-01T00:00:00Z",
      },
    ]);

    render(<CurrentUserInvitationsSection />);

    expect(screen.queryByText("1 pending"))
      .not.toBeInTheDocument();

    fireEvent.click(
      screen.getByRole("button", { name: "Expand invitations" })
    );

    expect(screen.getByText("Expired")).toBeInTheDocument();
  });

  it("displays an empty state when expanded", () =>
  {
    mockInvitations([]);

    render(<CurrentUserInvitationsSection />);

    fireEvent.click(
      screen.getByRole("button", { name: "Expand invitations" })
    );

    expect(screen.getByText("You have no invitations."))
      .toBeInTheDocument();
  });

  it("allows retrying after a loading error", () =>
  {
    mockInvitations([], { isError: true });

    render(<CurrentUserInvitationsSection />);

    fireEvent.click(
      screen.getByRole("button", { name: "Expand invitations" })
    );

    expect(screen.getByRole("alert")).toHaveTextContent(
      "Unable to load your invitations."
    );

    fireEvent.click(screen.getByRole("button", { name: "Retry" }));

    expect(mockRefetch).toHaveBeenCalledTimes(1);
  });
});