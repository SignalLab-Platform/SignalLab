import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { useRouter, useSearchParams } from "next/navigation";
import { HOME_ROUTE } from "@/navigation/navigation-routes";

import { ApiError } from "@/lib/api/api-error";

import { useOrganizationInvitation } from "./use-organization-invitation";
import { useCurrentUserInvitations } from "./use-current-user-invitations";
import { useAcceptOrganizationInvitation } from "./use-accept-organization-invitation";
import { useDeclineOrganizationInvitation } from "./use-decline-organization-invitation";

import { OrganizationInvitationPresentation } from "./organization-invitation-presentation";

import type {
  OrganizationInvitation,
  CurrentUserInvitation,
} from "./organization-invitation";

jest.mock("next/navigation", () =>
({
  useRouter: jest.fn(),
  useSearchParams: jest.fn(),
}));

jest.mock("./use-organization-invitation");
jest.mock("./use-current-user-invitations");
jest.mock("./use-accept-organization-invitation");
jest.mock("./use-decline-organization-invitation");

const invitationId = "invitation-1";

const mockPush = jest.fn();
const mockReplace = jest.fn();
const mockRefetch = jest.fn();
const mockAccept = jest.fn();
const mockDecline = jest.fn();

const invitation: OrganizationInvitation =
{
  id: invitationId,
  organizationId: "organization-1",
  invitedUserId: "user-2",
  invitedUserFirstName: "Alex",
  invitedUserLastName: "Martin",
  invitedUserEmail: "alex@example.com",
  invitedByUserId: "user-1",
  roleId: "role-member",
  roleName: "Member",
  status: "Pending",
  createdAt: "2026-01-01T10:00:00Z",
  updatedAt: "2026-01-01T10:00:00Z",
  expiresAt: "2099-01-08T10:00:00Z",
  respondedAt: null,
};

const personalInvitation: CurrentUserInvitation =
{
  id: invitationId,
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

function mockInvitationQuery(
  overrides: Partial<ReturnType<typeof useOrganizationInvitation>> = {}
): void
{
  jest.mocked(useOrganizationInvitation).mockReturnValue(
  {
    data: invitation,
    isPending: false,
    isError: false,
    refetch: mockRefetch,
    ...overrides,
  } as unknown as ReturnType<typeof useOrganizationInvitation>);
}

function renderPresentation(): void
{
  render(
    <OrganizationInvitationPresentation invitationId={invitationId} />
  );
}

describe("OrganizationInvitationPresentation", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();

    jest.mocked(useRouter).mockReturnValue(
    {
      push: mockPush,
      replace: mockReplace,
    } as unknown as ReturnType<typeof useRouter>);

    jest.mocked(useSearchParams).mockReturnValue(
      new URLSearchParams() as ReturnType<typeof useSearchParams>
    );

    mockRefetch.mockResolvedValue(undefined);
    mockAccept.mockResolvedValue(undefined);
    mockDecline.mockResolvedValue(undefined);

    mockInvitationQuery();

    jest.mocked(useCurrentUserInvitations).mockReturnValue(
    {
      data: [personalInvitation],
      isPending: false,
      isError: false,
    } as unknown as ReturnType<typeof useCurrentUserInvitations>);

    jest.mocked(useAcceptOrganizationInvitation).mockReturnValue(
    {
      mutateAsync: mockAccept,
      isPending: false,
    } as unknown as ReturnType<typeof useAcceptOrganizationInvitation>);

    jest.mocked(useDeclineOrganizationInvitation).mockReturnValue(
    {
      mutateAsync: mockDecline,
      isPending: false,
    } as unknown as ReturnType<typeof useDeclineOrganizationInvitation>);
  });

  it("displays the organization, role and privacy notice", () =>
  {
    renderPresentation();

    expect(
      screen.getByText("Organization Alpha")
    ).toBeInTheDocument();

    expect(
      screen.getByText("Member")
    ).toBeInTheDocument();

    expect(
      screen.getByText("Your information in this Organization")
    ).toBeInTheDocument();

    expect(
      screen.getByText(/email address associated with your SignalLab/i)
    ).toBeInTheDocument();

    expect(
      screen.getByRole("button", { name: "Accept invitation" })
    ).toBeEnabled();

    expect(
      screen.getByRole("button", { name: "Decline" })
    ).toBeEnabled();
  });

  it("displays the loading state", () =>
  {
    mockInvitationQuery(
    {
      data: undefined,
      isPending: true,
    });

    renderPresentation();

    expect(
      screen.getByRole("status")
    ).toHaveTextContent("Loading invitation...");
  });

  it("does not reveal information about an inaccessible invitation", () =>
  {
    mockInvitationQuery(
    {
      data: undefined,
      isError: true,
    });

    renderPresentation();

    expect(
      screen.getByText("Invitation unavailable")
    ).toBeInTheDocument();

    expect(
      screen.queryByText("Organization Alpha")
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole("button", { name: "Accept invitation" })
    ).not.toBeInTheDocument();
  });

  it.each(
  [
    ["Accepted", "already been accepted"],
    ["Declined", "declined this invitation"],
    ["Cancelled", "has been cancelled"],
    ["Expired", "has expired"],
  ] as const)(
    "displays %s invitations without response actions",
    (status, message) =>
    {
      mockInvitationQuery(
      {
        data:
        {
          ...invitation,
          status,
        },
      });

      renderPresentation();

      expect(
        screen.getByText(new RegExp(message))
      ).toBeInTheDocument();

      expect(
        screen.queryByRole("button", { name: "Accept invitation" })
      ).not.toBeInTheDocument();

      expect(
        screen.queryByRole("button", { name: "Decline" })
      ).not.toBeInTheDocument();
    }
  );

  it("treats a past pending invitation as expired", () =>
  {
    mockInvitationQuery(
    {
      data:
      {
        ...invitation,
        expiresAt: "2020-01-01T00:00:00Z",
      },
    });

    renderPresentation();

    expect(
      screen.getByText("This invitation has expired.")
    ).toBeInTheDocument();

    expect(
      screen.queryByRole("button", { name: "Accept invitation" })
    ).not.toBeInTheDocument();
  });

  it("accepts the invitation and navigates to its organization", async () =>
  {
    renderPresentation();

    fireEvent.click(
      screen.getByRole("button", { name: "Accept invitation" })
    );

    await waitFor(() =>
    {
      expect(mockAccept).toHaveBeenCalledTimes(1);
      expect(mockReplace).toHaveBeenCalledTimes(1);
    });

    const destination = mockReplace.mock.calls[0][0] as string;

    expect(destination).toContain("organization-1");
    expect(mockDecline).not.toHaveBeenCalled();
  });

  it("declines the invitation without navigating", async () =>
  {
    renderPresentation();

    fireEvent.click(
      screen.getByRole("button", { name: "Decline" })
    );

    await waitFor(() =>
    {
      expect(mockDecline).toHaveBeenCalledTimes(1);
    });

    expect(mockReplace).not.toHaveBeenCalled();
    expect(mockPush).not.toHaveBeenCalled();
  });

  it("displays a conflict when acceptance fails", async () =>
  {
    mockAccept.mockRejectedValue(
      new ApiError(
        409,
        "organization_invitation_not_pending",
        "Invitation is no longer pending."
      )
    );

    renderPresentation();

    fireEvent.click(
      screen.getByRole("button", { name: "Accept invitation" })
    );

    await waitFor(() =>
    {
      expect(screen.getByRole("alert")).toHaveTextContent(
        "This invitation is no longer available for this action."
      );
    });

    expect(mockReplace).not.toHaveBeenCalled();
    expect(mockRefetch).toHaveBeenCalled();
  });

  it("disables both actions while a response is pending", () =>
  {
    jest.mocked(useAcceptOrganizationInvitation).mockReturnValue(
    {
      mutateAsync: mockAccept,
      isPending: true,
    } as unknown as ReturnType<typeof useAcceptOrganizationInvitation>);

    renderPresentation();

    expect(
      screen.getByRole("button", { name: "Accepting..." })
    ).toBeDisabled();

    expect(
      screen.getByRole("button", { name: "Decline" })
    ).toBeDisabled();
  });

  it("returns to a verified local origin", () =>
  {
    jest.mocked(useSearchParams).mockReturnValue(
      new URLSearchParams(
      {
        from: "/home?tab=organizations",
      }) as ReturnType<typeof useSearchParams>
    );

    renderPresentation();

    fireEvent.click(
      screen.getByRole("button", { name: "Back" })
    );

    expect(mockPush).toHaveBeenCalledWith(
      "/home?tab=organizations"
    );
  });

  it("returns Home when the origin is unsafe", () =>
  {
    jest.mocked(useSearchParams).mockReturnValue(
      new URLSearchParams(
      {
        from: "https://example.com",
      }) as ReturnType<typeof useSearchParams>
    );

    renderPresentation();

    fireEvent.click(
      screen.getByRole("button", { name: "Back" })
    );

    expect(mockPush).toHaveBeenCalledWith(HOME_ROUTE);
  });
});