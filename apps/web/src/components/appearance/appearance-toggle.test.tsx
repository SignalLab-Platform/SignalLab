import
{
  fireEvent,
  render,
  screen,
} from "@testing-library/react";

import { AppearanceToggle } from "@/components/appearance/appearance-toggle";
import
{
  APPEARANCE_STORAGE_KEY,
  applyAppearanceMode,
} from "@/lib/appearance";
import { AppearanceProvider } from "@/providers/appearance-provider";

describe("AppearanceToggle", () =>
{
  beforeEach(() =>
  {
    window.localStorage.clear();
    document.documentElement.classList.remove("dark");
    delete document.documentElement.dataset.appearanceMode;
  });

  it("uses light mode by default", () =>
  {
    render(
      <AppearanceProvider>
        <AppearanceToggle />
      </AppearanceProvider>
    );

    const toggle = screen.getByRole("button",
    {
      name: "Switch to dark mode",
    });

    expect(toggle).toHaveAttribute("aria-pressed", "false");
    expect(document.documentElement).not.toHaveClass("dark");
  });

  it("switches to dark mode and persists the choice", () =>
  {
    render(
      <AppearanceProvider>
        <AppearanceToggle />
      </AppearanceProvider>
    );

    const toggle = screen.getByRole("button",
    {
      name: "Switch to dark mode",
    });

    fireEvent.click(toggle);

    expect(document.documentElement).toHaveClass("dark");
    expect(document.documentElement.dataset.appearanceMode).toBe("dark");
    expect(window.localStorage.getItem(APPEARANCE_STORAGE_KEY)).toBe("dark");

    expect(
      screen.getByRole("button",
      {
        name: "Switch to light mode",
      })
    ).toHaveAttribute("aria-pressed", "true");
  });

  it("restores an applied dark preference", () =>
  {
    window.localStorage.setItem(APPEARANCE_STORAGE_KEY, "dark");
    applyAppearanceMode("dark");

    render(
      <AppearanceProvider>
        <AppearanceToggle />
      </AppearanceProvider>
    );

    expect(
      screen.getByRole("button",
      {
        name: "Switch to light mode",
      })
    ).toHaveAttribute("aria-pressed", "true");
  });
});
