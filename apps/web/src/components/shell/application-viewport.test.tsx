import { render, screen } from "@testing-library/react";

import { ApplicationViewport } from "@/components/shell/application-viewport";
import { AppearanceProvider } from "@/providers/appearance-provider";

describe("ApplicationViewport", () =>
{
  beforeEach(() =>
  {
    window.localStorage.clear();

    document.documentElement.classList.remove("dark");

    delete document.documentElement.dataset.appearanceMode;
    delete document.documentElement.dataset.sidebarState;
  });

  it("renders the desktop application and its unsupported-device alternative", () =>
  {
    const { container } = render(
      <AppearanceProvider>
        <ApplicationViewport>
          <p>Current route content</p>
        </ApplicationViewport>
      </AppearanceProvider>
    );

    expect(
      container.querySelector("[data-desktop-application]")
    ).toBeInTheDocument();

    expect(
      container.querySelector("[data-desktop-required]")
    ).toBeInTheDocument();

    expect(
      screen.getByRole("heading",
      {
        level: 1,
        name: "Desktop browser required",
      })
    ).toBeInTheDocument();

    expect(screen.getByText("Current route content")).toBeInTheDocument();
  });
});
