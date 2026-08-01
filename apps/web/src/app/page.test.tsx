import { render, screen } from "@testing-library/react";

import Home from "./page";

describe("Home", () => {
  it("renders the SignalLab platform foundation", () => {
    render(<Home />);

    expect(
      screen.getByRole("heading", {
        level: 1,
        name: "SignalLab",
      }),
    ).toBeInTheDocument();

    expect(
      screen.getByText(
        "The frontend foundation is ready for the future application shell.",
      ),
    ).toBeInTheDocument();

    expect(
      screen.getByRole("button", {
        name: "Application shell not implemented",
      }),
    ).toBeDisabled();
  });
});
