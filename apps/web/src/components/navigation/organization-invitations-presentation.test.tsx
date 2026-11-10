import { fireEvent, render, screen, waitFor } from "@testing-library/react";

import { ApiError } from "@/lib/api/api-error";

import {
  OrganizationInvitationsActions,
  OrganizationInvitationsContent,
} from "@/components/navigation/organization-invitations-presentation";

import type {
  OrganizationInvitation,
} from "@/features/organization-invitations/organization-invitation";

import {
  useCancelOrganizationInvitation,
} from "@/features/organization-invitations/use-cancel-organization-invitation";

jest.mock(
  "@/features/organization-invitations/use-cancel-organization-invitation"
);

const mockedUseCancelOrganizationInvitation =
  jest.mocked(useCancelOrganizationInvitation);

const cancelMutation = jest.fn();

const invitation: OrganizationInvitation =
{
  id: "invitation-1",
  organizationId: "organization-1",
  invitedUserId: "user-2",
  invitedUserFirstName: "Alex",
  invitedUserLastName: "Martin",
  invitedUserEmail: "alex@example.com",
  invitedByUserId: "user-1",
  roleId: "role-1",
  roleName: "Member",
  status: "Pending",
  createdAt: "2026-01-01T10:00:00Z",
  updatedAt: "2026-01-01T10:00:00Z",
  expiresAt: "2099-01-08T10:00:00Z",
  respondedAt: null,
};

function renderContent(
  overrides: Partial<React.ComponentProps<typeof OrganizationInvitationsContent>> = {}
)
{
  const props =
  {
    organizationId: "organization-1",
    canManageInvitations: true,
    invitations: [invitation],
    isPending: false,
    isError: false,
    onRetry: jest.fn(),
    ...overrides,
  };

  render(<OrganizationInvitationsContent {...props} />);

  return props;
}

describe("OrganizationInvitationsPresentation", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();

    cancelMutation.mockResolvedValue(undefined);

    mockedUseCancelOrganizationInvitation.mockReturnValue(
    {
      mutateAsync: cancelMutation,
      isPending: false,
    } as unknown as ReturnType<typeof useCancelOrganizationInvitation>);
  });

  it("displays the invitation table and recipient information", () =>
  {
    renderContent();

    expect(screen.getByRole("table")).toBeInTheDocument();

    expect(screen.getByRole("columnheader", { name: "Recipient" }))
      .toBeInTheDocument();

    expect(screen.getByRole("columnheader", { name: "Role" }))
      .toBeInTheDocument();

    expect(screen.getByRole("columnheader", { name: "Status" }))
      .toBeInTheDocument();

    expect(screen.getByRole("columnheader", { name: "Invited at" }))
      .toBeInTheDocument();

    expect(screen.getByRole("columnheader", { name: "Expires at" }))
      .toBeInTheDocument();

    expect(screen.getByText("Alex Martin")).toBeInTheDocument();
    expect(screen.getByText("alex@example.com")).toBeInTheDocument();
    expect(screen.getByText("Pending")).toBeInTheDocument();
  });

  it("displays the empty state", () =>
  {
    renderContent({ invitations: [] });

    expect(screen.getByText("No invitations")).toBeInTheDocument();
    expect(screen.queryByRole("table")).not.toBeInTheDocument();
  });

  it("displays loading without showing stale invitations", () =>
  {
    renderContent({ isPending: true });

    expect(screen.getByRole("status"))
      .toHaveTextContent("Loading organization invitations...");

    expect(screen.queryByText("Alex Martin"))
      .not.toBeInTheDocument();
  });

  it("displays an error and allows retrying", () =>
  {
    const props = renderContent({ isError: true });

    expect(screen.getByRole("alert"))
      .toHaveTextContent("Unable to load organization invitations.");

    fireEvent.click(screen.getByRole("button", { name: "Retry" }));

    expect(props.onRetry).toHaveBeenCalledTimes(1);
  });

  it("displays the effective Expired status", () =>
  {
    renderContent(
    {
      invitations:
      [
        {
          ...invitation,
          expiresAt: "2020-01-01T00:00:00Z",
        },
      ],
    });

    expect(screen.getByText("Expired")).toBeInTheDocument();
    expect(screen.queryByText("Pending")).not.toBeInTheDocument();
  });

  it("hides the creation action without ManageMembers permission", () =>
  {
    render(
      <OrganizationInvitationsActions
        canInvite={false}
        onInvite={jest.fn()}
      />
    );

    expect(
      screen.queryByRole("button", { name: "Invite member" })
    ).not.toBeInTheDocument();
  });

  it("allows authorized users to open the invitation dialog", () =>
  {
    const onInvite = jest.fn();

    render(
      <OrganizationInvitationsActions
        canInvite={true}
        onInvite={onInvite}
      />
    );

    const button = screen.getByRole("button", { name: "Invite member" });

    expect(button).toBeEnabled();

    fireEvent.click(button);

    expect(onInvite).toHaveBeenCalledTimes(1);
  });

  it("hides invitation actions without ManageMembers permission", () =>
  {
    renderContent({ canManageInvitations: false });

    expect(
      screen.queryByRole("button", { name: /Copy invitation link for/ })
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole("button", { name: /Cancel invitation for/ })
    ).not.toBeInTheDocument();
  });

  it("does not allow cancelling a completed invitation", () =>
  {
    renderContent(
    {
      invitations:
      [
        {
          ...invitation,
          status: "Accepted",
        },
      ],
    });

    expect(
      screen.queryByRole("button", { name: /Cancel invitation for/ })
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole("textbox", {
        name: "Invitation link for alex@example.com",
      })
    ).toBeInTheDocument();
  });

  it("opens a confirmation before cancelling", async () =>
  {
    renderContent();

    fireEvent.click(
      screen.getByRole("button", {
        name: "Cancel invitation for alex@example.com",
      })
    );

    expect(
      screen.getByRole("alertdialog")
    ).toBeInTheDocument();

    expect(cancelMutation).not.toHaveBeenCalled();

    fireEvent.click(
      screen.getByRole("button", {
        name: /^Cancel invitation$/,
      })
    );

    await waitFor(() =>
    {
      expect(cancelMutation).toHaveBeenCalledWith("invitation-1");
    });
  });

  it("copies the link of an existing invitation", async () =>
  {
    const writeText = jest.fn().mockResolvedValue(undefined);

    Object.defineProperty(navigator, "clipboard",
    {
      configurable: true,
      value: { writeText },
    });

    renderContent();

    fireEvent.click(
      screen.getByRole("button", {
        name: "Copy invitation link for alex@example.com",
      })
    );

    await waitFor(() =>
    {
      expect(writeText).toHaveBeenCalledWith(
        `${window.location.origin}/organization-invitations/invitation-1`
      );
    });

    expect(screen.getByText("Copied")).toBeInTheDocument();
  });

  it("keeps the confirmation open when cancellation fails", async () =>
  {
    cancelMutation.mockRejectedValue(
      new ApiError(
        409,
        "organization_invitation_not_pending",
        "The invitation is no longer pending."
      )
    );

    renderContent();

    fireEvent.click(
      screen.getByRole("button", {
        name: "Cancel invitation for alex@example.com",
      })
    );

    fireEvent.click(
      screen.getByRole("button", {
        name: /^Cancel invitation$/,
      })
    );

    await waitFor(() =>
    {
      expect(screen.getByRole("alert")).toHaveTextContent(
        "This invitation can no longer be cancelled."
      );
    });

    expect(screen.getByRole("alertdialog")).toBeInTheDocument();

    expect(cancelMutation).toHaveBeenCalledWith("invitation-1");
  });

  it("displays an error when copying an invitation link fails", async () =>
  {
    const writeText = jest.fn().mockRejectedValue(
      new Error("Clipboard access denied.")
    );

    Object.defineProperty(navigator, "clipboard",
    {
      configurable: true,
      value: { writeText },
    });

    renderContent();

    fireEvent.click(
      screen.getByRole("button", {
        name: "Copy invitation link for alex@example.com",
      })
    );

    await waitFor(() =>
    {
      expect(screen.getByRole("alert")).toHaveTextContent(
        "Automatic copying failed. Select the invitation link to copy it manually."
      );
    });

    expect(screen.queryByText("Copied")).not.toBeInTheDocument();

    expect(
      screen.getByRole("textbox", {
        name: "Invitation link for alex@example.com",
      })
    ).toBeInTheDocument();
  });

  it("always exposes a selectable invitation link", () =>
  {
    renderContent();

    expect(
      screen.getByRole("textbox", {
        name: "Invitation link for alex@example.com",
      })
    ).toHaveValue(
      `${window.location.origin}/organization-invitations/invitation-1`
    );

    expect(
      screen.getByRole("textbox", {
        name: "Invitation link for alex@example.com",
      })
    ).toHaveAttribute("readonly");
  });
});