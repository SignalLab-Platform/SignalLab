import { render, screen } from "@testing-library/react";
import { usePathname } from "next/navigation";

import { ContextNavigation } from "@/components/navigation/context-navigation";

import
{
  resetNavigationState,
  updateNavigationResourceState,
} from "@/navigation/navigation-state";

jest.mock("next/navigation", () =>
{
  return {
    usePathname: jest.fn(),
  };
});

describe("ContextNavigation", () =>
{
  const mockedUsePathname = jest.mocked(usePathname);

  beforeEach(() =>
  {
    mockedUsePathname.mockReturnValue("/home");
    resetNavigationState();
  });

  it("renders empty resource sections on Personal Home", () =>
  {
    render(<ContextNavigation />);

    expect(
      screen.getByRole("link",
      {
        name: "Home",
      })
    ).toHaveAttribute("href", "/home");

    expect(
      screen.getByText("Workspaces")
    ).toBeInTheDocument();

    expect(
      screen.getByText("No workspaces yet.")
    ).toBeInTheDocument();

    expect(
      screen.getByText("My participations")
    ).toBeInTheDocument();

    expect(
      screen.getByText("No participations yet.")
    ).toBeInTheDocument();

    expect(
      screen.queryByRole("link",
      {
        name: "Demo Organization",
      })
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole("link",
      {
        name: "Demo Project",
      })
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole("link",
      {
        name: "Demo Campaign",
      })
    ).not.toBeInTheDocument();

    expect(
      screen.queryByRole("link",
      {
        name: "Demo Participation",
      })
    ).not.toBeInTheDocument();
  });

  it("identifies the current Campaign and its contextual ancestors", () =>
  {
    mockedUsePathname.mockReturnValue(
      "/organizations/demo-organization/projects/demo-project/campaigns/demo-campaign/results"
    );

    render(<ContextNavigation />);

    expect(
      screen.getByRole("link",
      {
        name: "Demo Organization",
      })
    ).toHaveAttribute("data-context-status", "ancestor");

    expect(
      screen.getByRole("link",
      {
        name: "Demo Project",
      })
    ).toHaveAttribute("data-context-status", "ancestor");

    expect(
      screen.getByRole("link",
      {
        name: "Demo Campaign",
      })
    ).toHaveAttribute("data-context-status", "current");

    expect(
      screen.getByRole("link",
      {
        name: "Demo Campaign",
      })
    ).toHaveAttribute("aria-current", "location");
  });

  it("keeps Participation outside the Organization hierarchy", () =>
  {
    mockedUsePathname.mockReturnValue(
      "/participations/demo-participation/consent"
    );

    render(<ContextNavigation />);

    expect(
      screen.getByRole("link",
      {
        name: "Demo Participation",
      })
    ).toHaveAttribute("data-context-status", "current");

    expect(
      screen.getByRole("link",
      {
        name: "Demo Organization",
      })
    ).toHaveAttribute("data-context-status", "available");

    expect(
      screen.getByRole("link",
      {
        name: "Demo Project",
      })
    ).toHaveAttribute("data-context-status", "available");
  });

  it("reopens each resource on its last valid local view", () =>
  {
    mockedUsePathname.mockReturnValue(
      "/organizations/demo-organization/projects/demo-project/campaigns/demo-campaign/overview"
    );

    updateNavigationResourceState(
      "organization:demo-organization",
      {
        lastLocalView: "projects",
      }
    );

    updateNavigationResourceState(
      "project:demo-project",
      {
        lastLocalView: "analyses",
      }
    );

    updateNavigationResourceState(
      "campaign:demo-campaign",
      {
        lastLocalView: "results",
      }
    );

    updateNavigationResourceState(
      "participation:demo-participation",
      {
        lastLocalView: "support",
      }
    );

    render(<ContextNavigation />);

    expect(
      screen.getByRole("link",
      {
        name: "Demo Organization",
      })
    ).toHaveAttribute(
      "href",
      "/organizations/demo-organization/projects"
    );

    expect(
      screen.getByRole("link",
      {
        name: "Demo Project",
      })
    ).toHaveAttribute(
      "href",
      "/organizations/demo-organization/projects/demo-project/analyses"
    );

    expect(
      screen.getByRole("link",
      {
        name: "Demo Campaign",
      })
    ).toHaveAttribute(
      "href",
      "/organizations/demo-organization/projects/demo-project/campaigns/demo-campaign/results"
    );

    expect(
      screen.getByRole("link",
      {
        name: "Demo Participation",
      })
    ).toHaveAttribute(
      "href",
      "/participations/demo-participation/support"
    );
  });
});
