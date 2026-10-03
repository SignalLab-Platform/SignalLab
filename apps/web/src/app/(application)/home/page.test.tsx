import { render, screen } from "@testing-library/react";

import { useCurrentUser } from "@/features/current-user/use-current-user";

import HomePage from "./page";

jest.mock("@/features/current-user/use-current-user");

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
      screen.getByText("No organizations available yet.")
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
});
