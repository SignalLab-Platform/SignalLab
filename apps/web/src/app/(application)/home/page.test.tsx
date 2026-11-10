import { render, screen } from "@testing-library/react";

import { useCurrentUser } from "@/features/current-user/use-current-user";

import HomePage from "./page";

jest.mock("@/features/current-user/use-current-user");

jest.mock(
  "@/features/organizations/organizations-personal-explorer",
  () =>
  {
    return {
      OrganizationsPersonalExplorer: () =>
      {
        return (
          <section aria-labelledby="organizations-heading">
            <h2 id="organizations-heading">
              Organizations
            </h2>
          </section>
        );
      },
    };
  }
);

jest.mock(
  "@/features/organization-invitations/current-user-invitations-section",
  () =>
  ({
    CurrentUserInvitationsSection: () => (
      <section aria-labelledby="invitations-heading">
        <h2 id="invitations-heading">
          Invitations
        </h2>
      </section>
    ),
  })
);

describe("HomePage", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();

    jest.mocked(useCurrentUser).mockReturnValue(
    {
      data:
      {
        id: "7e34f695-46df-4c29-9852-030783798531",
        email: "user@example.com",
      },
      isPending: false,
      isError: false,
    } as ReturnType<typeof useCurrentUser>);
  });

  it("renders the current user's Personal Home", () =>
  {
    render(<HomePage />);

    expect(
      screen.getByRole("heading",
      {
        level: 1,
        name: "Personal Home",
      })
    ).toBeInTheDocument();

    expect(
      screen.getByText("user@example.com")
    ).toBeInTheDocument();

    expect(
      screen.getByRole("heading",
      {
        level: 2,
        name: "Organizations",
      })
    ).toBeInTheDocument();

    expect(
      screen.getByRole("region",
      {
        name: "Organizations",
      })
    ).toBeInTheDocument();

    expect(
      screen.getByRole("heading",
      {
        level: 2,
        name: "Participations",
      })
    ).toBeInTheDocument();

    expect(
      screen.getByText("No participations available yet.")
    ).toBeInTheDocument();

    expect(
      screen.queryByText("Demo Organization")
    ).not.toBeInTheDocument();

    expect(
      screen.queryByText("Demo Participation")
    ).not.toBeInTheDocument();
  });

  it("places Invitations between Organizations and Participations", () =>
  {
    render(<HomePage />);

    const organizations = screen.getByRole("heading",
    {
      name: "Organizations",
    });

    const invitations = screen.getByRole("heading",
    {
      name: "Invitations",
    });

    const participations = screen.getByRole("heading",
    {
      name: "Participations",
    });

    expect(
      organizations.compareDocumentPosition(invitations) &
      Node.DOCUMENT_POSITION_FOLLOWING
    ).toBeTruthy();

    expect(
      invitations.compareDocumentPosition(participations) &
      Node.DOCUMENT_POSITION_FOLLOWING
    ).toBeTruthy();
  });
});
