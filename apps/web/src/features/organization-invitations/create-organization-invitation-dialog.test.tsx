import { fireEvent, render, screen, waitFor } from "@testing-library/react";

import { ApiError } from "@/lib/api/api-error";

import type {
  OrganizationRole,
} from "@/features/organizations/organization-role";

import {
  useCreateOrganizationInvitation,
} from "./use-create-organization-invitation";

import {
  CreateOrganizationInvitationDialog,
} from "./create-organization-invitation-dialog";

jest.mock("./use-create-organization-invitation");

jest.mock("@/components/ui/select", () =>
{
  return {
    Select: (
      {
        children,
        value,
        onValueChange,
        disabled,
      }: {
        children: React.ReactNode;
        value: string;
        onValueChange: (value: string) => void;
        disabled?: boolean;
      }
    ) =>
    {
      return (
        <select
          value={value}
          onChange={(event) => onValueChange(event.target.value)}
          disabled={disabled}
        >
          {children}
        </select>
      );
    },

    SelectTrigger: () => null,
    SelectValue: () => null,

    SelectContent: (
      { children }: { children: React.ReactNode }
    ) => children,

    SelectItem: (
      {
        children,
        value,
      }: {
        children: React.ReactNode;
        value: string;
      }
    ) =>
    {
      return <option value={value}>{children}</option>;
    },
  };
});

const roles: OrganizationRole[] =
[
  {
    id: "role-owner",
    name: "Owner",
    description: null,
    isOwner: true,
    permissions: ["ManageMembers"],
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

const mutateAsync = jest.fn();
const reset = jest.fn();
const onOpenChange = jest.fn();

function mockMutation(
  overrides: Record<string, unknown> = {}
): void
{
  jest.mocked(useCreateOrganizationInvitation).mockReturnValue(
  {
    mutateAsync,
    reset,
    isPending: false,
    isError: false,
    error: null,
    ...overrides,
  } as unknown as ReturnType<typeof useCreateOrganizationInvitation>);
}

function renderDialog(canInviteOwner = false): void
{
  render(
    <CreateOrganizationInvitationDialog
      organizationId="organization-1"
      roles={roles}
      open={true}
      onOpenChange={onOpenChange}
      canInviteOwner={canInviteOwner}
    />
  );
}

function fillForm(): void
{
  fireEvent.change(
    screen.getByRole("textbox", { name: "Email address" }),
    {
      target: { value: "alex@example.com" },
    }
  );

  const selects = screen.getAllByRole("combobox");

  fireEvent.change(selects[0], {
    target: { value: "role-member" },
  });
}

describe("CreateOrganizationInvitationDialog", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();

    mockMutation();

    mutateAsync.mockResolvedValue(
    {
      id: "invitation-123",
    });
  });

  it("requires an email and role before submission", () =>
  {
    renderDialog();

    expect(
      screen.getByRole("button", { name: "Send invitation" })
    ).toBeDisabled();

    fillForm();

    expect(
      screen.getByRole("button", { name: "Send invitation" })
    ).toBeEnabled();
  });

  it("creates an invitation and displays its shareable link", async () =>
  {
    renderDialog();

    fillForm();

    fireEvent.click(
      screen.getByRole("button", { name: "Send invitation" })
    );

    await waitFor(() =>
    {
      expect(mutateAsync).toHaveBeenCalledWith(
      {
        email: "alex@example.com",
        roleId: "role-member",
        duration: 7,
      });
    });

    expect(
      await screen.findByText("Invitation sent")
    ).toBeInTheDocument();

    expect(
      screen.getByRole("textbox", { name: "Invitation link" })
    ).toHaveValue(
      `${window.location.origin}/organization-invitations/invitation-123`
    );

    fireEvent.click(
      screen.getByRole("button", { name: "Done" })
    );

    expect(reset).toHaveBeenCalled();
    expect(onOpenChange).toHaveBeenCalledWith(false);
  });

  it("shows the backend error without clearing the form", () =>
  {
    mockMutation(
    {
      isError: true,
      error: new ApiError(
        409,
        "organization_invitation_already_pending",
        "An invitation is already pending."
      ),
    });

    renderDialog();

    fillForm();

    expect(screen.getByRole("alert")).toHaveTextContent(
      "An invitation is already pending"
    );

    expect(
      screen.getByRole("textbox", { name: "Email address" })
    ).toHaveValue("alex@example.com");
  });

  it("does not allow submission while the mutation is pending", () =>
  {
    mockMutation({ isPending: true });

    renderDialog();

    fillForm();

    expect(
      screen.getByRole("button", { name: "Sending..." })
    ).toBeDisabled();

    expect(
      screen.getByRole("button", { name: "Cancel" })
    ).toBeDisabled();
  });

  it("copies the invitation link to the clipboard", async () =>
  {
    const writeText = jest.fn().mockResolvedValue(undefined);

    Object.defineProperty(navigator, "clipboard",
    {
      configurable: true,
      value: { writeText },
    });

    renderDialog();

    fillForm();

    fireEvent.click(
      screen.getByRole("button", { name: "Send invitation" })
    );

    await screen.findByText("Invitation sent");

    fireEvent.click(
      screen.getByRole("button", { name: "Copy invitation link" })
    );

    await waitFor(() =>
    {
      expect(writeText).toHaveBeenCalledWith(
        `${window.location.origin}/organization-invitations/invitation-123`
      );
    });

    expect(
      await screen.findByText("Link copied to clipboard.")
    ).toBeInTheDocument();
  });

  it("does not offer the Owner role to a non-owner", async () =>
  {
    renderDialog(false);

    fireEvent.change(
      screen.getByRole("textbox", { name: "Email address" }),
      {
        target: { value: "owner@example.com" },
      }
    );

    expect(
      screen.queryByRole("option", { name: "Owner" })
    ).not.toBeInTheDocument();
  });

  it("offers the Owner role to an owner", async () =>
  {
    renderDialog(true);

    fireEvent.change(
      screen.getByRole("textbox", { name: "Email address" }),
      {
        target: { value: "owner@example.com" },
      }
    );

    expect(
      screen.getByRole("option", { name: "Owner" })
    ).toBeInTheDocument();
  });
});