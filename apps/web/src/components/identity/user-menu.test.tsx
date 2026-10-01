import { render, screen } from "@testing-library/react";

import { UserMenu } from "@/components/identity/user-menu";

jest.mock("@clerk/nextjs", () =>
{
  return {
    UserButton: ({ showName }: { showName?: boolean }) =>
    {
      return (
        <div
          data-testid="clerk-user-button"
          data-show-name={String(showName)}
        >
          User button
        </div>
      );
    },
  };
});

describe("UserMenu", () =>
{
  it("renders the Clerk user button with the user name", () =>
  {
    render(<UserMenu />);

    expect(
      screen.getByTestId("clerk-user-button")
    ).toHaveAttribute("data-show-name", "true");
  });
});