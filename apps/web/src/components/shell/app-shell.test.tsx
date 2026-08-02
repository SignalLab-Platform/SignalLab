import { render, screen } from "@testing-library/react";

import { AppShell } from "@/components/shell/app-shell";
import { AppearanceProvider } from "@/providers/appearance-provider";

describe("AppShell", () =>
{
  beforeEach(() =>
  {
    window.localStorage.clear();
    document.documentElement.classList.remove("dark");
    delete document.documentElement.dataset.appearanceMode;
  });

  it("renders the permanent application regions", () =>
  {
    render(
      <AppearanceProvider>
        <AppShell>
          <p>Current route content</p>
        </AppShell>
      </AppearanceProvider>
    );

    expect(screen.getByRole("banner")).toBeInTheDocument();

    expect(
      screen.getByRole("navigation",
      {
        name: "Context navigation",
      })
    ).toBeInTheDocument();

    expect(screen.getByRole("main")).toBeInTheDocument();

    expect(
      screen.getByText("Current route content")
    ).toBeInTheDocument();

    expect(
      screen.getByRole("button",
      {
        name: "Switch to dark mode",
      })
    ).toBeInTheDocument();
  });
});
