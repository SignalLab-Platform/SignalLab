import { render, screen } from "@testing-library/react";

import { useCurrentUser } from "@/features/current-user/use-current-user";

import { NavigationPresentation } from "@/components/navigation/navigation-presentation";
import { resolveNavigationLocation } from "@/navigation/navigation-resolver";

import
{
  resetNavigationState,
} from "@/navigation/navigation-state";

function resolveRequiredLocation(pathname: string)
{
  const location = resolveNavigationLocation(pathname);

  if (location === null)
  {
    throw new Error(`Unable to resolve test pathname: ${pathname}`);
  }

  return location;
}

jest.mock("next/navigation", () =>
{
  return {
    usePathname: () => "/home",

    useSearchParams: () =>
      new URLSearchParams(),

    useRouter: () =>
    {
      return {
        push: jest.fn(),
        back: jest.fn(),
        replace: jest.fn(),
      };
    },
  };
});

jest.mock("@/features/current-user/use-current-user");

describe("NavigationPresentation", () =>
{
  beforeEach(() =>
  {
    resetNavigationState();

    jest.mocked(useCurrentUser).mockReturnValue(
    {
      data:
      {
        id: "7e34f695-46df-4c29-9852-030783798531",
        email: "user@example.com",
      },
      isPending: false,
      isError: false,
    } as unknown as ReturnType<typeof useCurrentUser>);
  });

  it("renders an Explorer with Search, tabs, actions, and resources", () =>
  {
    const location = resolveRequiredLocation(
      "/organizations/demo-organization/projects"
    );

    const { container } = render(
      <NavigationPresentation location={location} />
    );

    expect(
      container.querySelector(
        '[data-sticky-presentation-bar="explorer"]'
      )
    ).toBeInTheDocument();

    expect(
      screen.getByRole("heading",
      {
        level: 1,
        name: "Demo Organization",
      })
    ).toBeInTheDocument();

    expect(
      screen.getByRole("searchbox",
      {
        name: "Search Demo Organization",
      })
    ).toBeInTheDocument();

    expect(
      screen.getByRole("navigation",
      {
        name: "Local navigation",
      })
    ).toBeInTheDocument();

    expect(
      screen.getByRole("button",
      {
        name: "Create resource",
      })
    ).toBeDisabled();

    expect(
      screen.getByRole("link",
      {
        name: /Demo Project/,
      })
    ).toBeInTheDocument();
  });

  it("renders a compact Campaign Workspace identity and tabs", () =>
  {
    const location = resolveRequiredLocation(
      "/organizations/demo-organization/projects/demo-project/campaigns/demo-campaign/results"
    );

    const { container } = render(
      <NavigationPresentation location={location} />
    );

    expect(
      container.querySelector(
        '[data-sticky-presentation-bar="workspace"]'
      )
    ).toBeInTheDocument();

    expect(
      screen.getByRole("heading",
      {
        level: 1,
        name: "Demo Campaign",
      })
    ).toBeInTheDocument();

    expect(
      screen.getByRole("link",
      {
        name: "Results",
      })
    ).toHaveAttribute("aria-current", "page");

    expect(
      screen.getByRole("heading",
      {
        level: 2,
        name: "Results view",
      })
    ).toBeInTheDocument();
  });

  it("renders a continuous Document with a Table of Contents", () =>
  {
    const location = resolveRequiredLocation(
      "/participations/demo-participation/consent"
    );

    const { container } = render(
      <NavigationPresentation location={location} />
    );

    expect(
      container.querySelector(
        '[data-sticky-presentation-bar="document"]'
      )
    ).toBeInTheDocument();

    expect(
      screen.getByRole("heading",
      {
        level: 1,
        name: "Demo Participation",
      })
    ).toBeInTheDocument();

    expect(
      screen.getByRole("navigation",
      {
        name: "Table of contents",
      })
    ).toBeInTheDocument();

    expect(
      screen.getByRole("link",
      {
        name: "Consent",
      })
    ).toHaveAttribute("aria-current", "location");

    expect(
      screen.getByRole("heading",
      {
        level: 2,
        name: "Overview",
      })
    ).toBeInTheDocument();

    expect(
      screen.getByRole("heading",
      {
        level: 2,
        name: "Support",
      })
    ).toBeInTheDocument();

    expect(
      screen.queryByRole("navigation",
      {
        name: "Local navigation",
      })
    ).not.toBeInTheDocument();
  });

  it("renders Personal Home without Local Navigation", () =>
  {
    const location = resolveRequiredLocation("/home");

    render(
      <NavigationPresentation location={location} />
    );

    expect(
      screen.getByRole("heading",
      {
        level: 1,
        name: "Personal Home",
      })
    ).toBeInTheDocument();

    expect(
      screen.queryByRole("navigation",
      {
        name: "Local navigation",
      })
    ).not.toBeInTheDocument();
  });
});
