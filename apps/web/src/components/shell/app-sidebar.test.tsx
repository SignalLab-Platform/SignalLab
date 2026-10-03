import
{
  fireEvent,
  render,
  screen,
} from "@testing-library/react";

import { AppSidebar } from "@/components/shell/app-sidebar";
import
{
  SIDEBAR_STORAGE_KEY,
  applySidebarState,
} from "@/lib/sidebar";

jest.mock("@/components/identity/user-menu", () =>
{
  return {
    UserMenu: () => <div data-testid="user-menu">User menu</div>,
  };
});

jest.mock("next/navigation", () =>
{
  return {
    usePathname: () => "/home",
  };
});

describe("AppSidebar", () =>
{
  beforeEach(() =>
  {
    window.localStorage.clear();
    delete document.documentElement.dataset.sidebarState;
  });

  it("uses the expanded state by default", () =>
  {
    render(<AppSidebar />);

    const toggle = screen.getByRole("button",
    {
      name: "Collapse sidebar",
    });

    expect(toggle).toHaveAttribute("aria-expanded", "true");

    expect(
      screen.getByRole("link",
      {
        name: "Home",
      })
    ).toHaveAttribute("aria-current", "location");
  });

  it("collapses the sidebar and persists the choice", () =>
  {
    render(<AppSidebar />);

    const toggle = screen.getByRole("button",
    {
      name: "Collapse sidebar",
    });

    fireEvent.click(toggle);

    expect(document.documentElement.dataset.sidebarState).toBe("collapsed");
    expect(window.localStorage.getItem(SIDEBAR_STORAGE_KEY)).toBe("collapsed");

    expect(
      screen.getByRole("button",
      {
        name: "Expand sidebar",
      })
    ).toHaveAttribute("aria-expanded", "false");
  });

  it("restores a stored collapsed state", () =>
  {
    window.localStorage.setItem(SIDEBAR_STORAGE_KEY, "collapsed");
    applySidebarState("collapsed");

    render(<AppSidebar />);

    expect(
      screen.getByRole("button",
      {
        name: "Expand sidebar",
      })
    ).toHaveAttribute("aria-expanded", "false");
  });
});
