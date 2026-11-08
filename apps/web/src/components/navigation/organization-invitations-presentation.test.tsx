import { render, screen } from "@testing-library/react";

import
{
  OrganizationInvitationsActions,
  OrganizationInvitationsContent,
} from "@/components/navigation/organization-invitations-presentation";

describe("OrganizationInvitationsPresentation", () =>
{
  it("displays the invitation table structure", () =>
  {
    render(<OrganizationInvitationsContent />);

    expect(
      screen.getByRole("columnheader", { name: "Email" })
    ).toBeInTheDocument();

    expect(
      screen.getByRole("columnheader", { name: "Role" })
    ).toBeInTheDocument();

    expect(
      screen.getByRole("columnheader", { name: "Status" })
    ).toBeInTheDocument();

    expect(
      screen.getByRole("columnheader", { name: "Invited at" })
    ).toBeInTheDocument();

    expect(
      screen.getByRole("columnheader", { name: "Actions" })
    ).toBeInTheDocument();
  });

  it("distinguishes unavailable data from an empty invitation list", () =>
  {
    render(<OrganizationInvitationsContent />);

    expect(
      screen.getByRole("status")
    ).toHaveTextContent("Invitation data is not available yet.");

    expect(
      screen.getByText("Invitations are not available yet")
    ).toBeInTheDocument();

    expect(
      screen.queryByText("No invitations")
    ).not.toBeInTheDocument();

    expect(
      screen.queryAllByRole("row")
    ).toHaveLength(1);
  });

  it("keeps the invitation command disabled", () =>
  {
    render(<OrganizationInvitationsActions />);

    expect(
      screen.getByRole("button", { name: "Invite member" })
    ).toBeDisabled();
  });
});