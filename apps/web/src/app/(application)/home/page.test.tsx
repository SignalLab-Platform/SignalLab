import { render, screen } from "@testing-library/react";

import HomePage from "./page";

describe("HomePage", () =>
{
  it("renders the Personal Home presentation", () =>
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
      screen.getByRole("link",
      {
        name: /Demo Organization/,
      })
    ).toBeInTheDocument();

    expect(
      screen.getByRole("link",
      {
        name: /Demo Participation/,
      })
    ).toBeInTheDocument();
  });
});
