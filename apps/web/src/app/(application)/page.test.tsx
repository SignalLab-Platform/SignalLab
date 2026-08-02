import { render, screen } from "@testing-library/react";

import Home from "./page";

describe("Home", () =>
{
  it("renders the application shell foundation content", () =>
  {
    render(<Home />);

    expect(
      screen.getByRole("heading",
      {
        level: 1,
        name: "Application shell",
      })
    ).toBeInTheDocument();

    expect(
      screen.getByText(
        "Header, Sidebar, and Content are now hosted by a persistent application layout."
      )
    ).toBeInTheDocument();

    expect(
      screen.getByRole("heading",
      {
        level: 2,
        name: "Header",
      })
    ).toBeInTheDocument();

    expect(
      screen.getByRole("heading",
      {
        level: 2,
        name: "Sidebar",
      })
    ).toBeInTheDocument();

    expect(
      screen.getByRole("heading",
      {
        level: 2,
        name: "Content",
      })
    ).toBeInTheDocument();
  });
});
