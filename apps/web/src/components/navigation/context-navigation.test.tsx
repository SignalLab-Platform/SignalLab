import { act, fireEvent, render, screen } from "@testing-library/react";

import { usePathname } from "next/navigation";

import { ContextNavigation } from "@/components/navigation/context-navigation";

import { useOrganizations } from "@/features/organizations/use-organizations";

import type
{
  Organization,
} from "@/features/organizations/organization";

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

jest.mock("@/features/organizations/use-organizations", () =>
{
  return {
    useOrganizations: jest.fn(),
  };
});

describe("ContextNavigation", () =>
{
  const mockedUsePathname = jest.mocked(usePathname);
  const mockedUseOrganizations = jest.mocked(useOrganizations);

  const organizations: Organization[] =
  [
    {
      id: "organization-alpha",
      name: "Studio Alpha",
      country: "FR",
      website: null,
    },
    {
      id: "organization-beta",
      name: "Studio Beta",
      country: "FR",
      website: null,
    },
  ];

  function mockOrganizations(
    data: Organization[] = organizations
  ): void
  {
    mockedUseOrganizations.mockReturnValue(
    {
      data,
      isPending: false,
      isError: false,
    } as ReturnType<typeof useOrganizations>);
  }

  beforeEach(() =>
  {
    mockedUsePathname.mockReturnValue("/home");
    mockedUseOrganizations.mockReset();

    resetNavigationState();
    mockOrganizations();
  });

  it("renders real Organizations on Personal Home", () =>
  {
    render(<ContextNavigation />);

    expect(
      screen.getByRole("link",
      {
        name: "Home",
      })
    ).toHaveAttribute("aria-current", "location");

    expect(
      screen.getByRole("link",
      {
        name: "Studio Alpha",
      })
    ).toHaveAttribute(
      "href",
      "/organizations/organization-alpha/all"
    );

    expect(
      screen.getByRole("link",
      {
        name: "Studio Beta",
      })
    ).toHaveAttribute(
      "href",
      "/organizations/organization-beta/all"
    );

    expect(
      screen.getByText("Workspaces")
    ).toBeInTheDocument();

    expect(
      screen.getByText("My participations")
    ).toBeInTheDocument();

    expect(
      screen.getByText("No participations yet.")
    ).toBeInTheDocument();

    expect(
      screen.queryByText("No workspaces yet.")
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
  });

  it("identifies the current Organization", () =>
  {
    mockedUsePathname.mockReturnValue(
      "/organizations/organization-beta/measures"
    );

    render(<ContextNavigation />);

    expect(
      screen.getByRole("link",
      {
        name: "Studio Beta",
      })
    ).toHaveAttribute("data-context-status", "current");

    expect(
      screen.getByRole("link",
      {
        name: "Studio Beta",
      })
    ).toHaveAttribute("aria-current", "location");

    expect(
      screen.getByRole("link",
      {
        name: "Studio Alpha",
      })
    ).toHaveAttribute("data-context-status", "available");
  });

  it("identifies the Organization as current within a Campaign", () =>
  {
    mockedUsePathname.mockReturnValue(
      "/organizations/organization-alpha/projects/project-one/campaigns/campaign-one/results"
    );

    render(<ContextNavigation />);

    expect(
      screen.getByRole("link",
      {
        name: "Studio Alpha",
      })
    ).toHaveAttribute("data-context-status", "current");

    expect(
      screen.getByRole("link",
      {
        name: "Studio Beta",
      })
    ).toHaveAttribute("data-context-status", "available");

    expect(
      screen.queryByRole("link",
      {
        name: "Demo Campaign",
      })
    ).not.toBeInTheDocument();
  });

  it("keeps Participation outside the Organization hierarchy", () =>
  {
    mockedUsePathname.mockReturnValue(
      "/participations/participation-one/consent"
    );

    render(<ContextNavigation />);

    expect(
      screen.getByRole("link",
      {
        name: "Studio Alpha",
      })
    ).toHaveAttribute("data-context-status", "available");

    expect(
      screen.getByRole("link",
      {
        name: "Studio Beta",
      })
    ).toHaveAttribute("data-context-status", "available");

    expect(
      screen.getByText("No participations yet.")
    ).toBeInTheDocument();
  });

  it("reopens each Organization on its last valid local view", () =>
  {
    updateNavigationResourceState(
      "organization:organization-alpha",
      {
        lastLocalView: "projects",
      }
    );

    updateNavigationResourceState(
      "organization:organization-beta",
      {
        lastLocalView: "measures",
      }
    );

    render(<ContextNavigation />);

    expect(
      screen.getByRole("link",
      {
        name: "Studio Alpha",
      })
    ).toHaveAttribute(
      "href",
      "/organizations/organization-alpha/projects"
    );

    expect(
      screen.getByRole("link",
      {
        name: "Studio Beta",
      })
    ).toHaveAttribute(
      "href",
      "/organizations/organization-beta/measures"
    );
  });

  it("falls back to the default view when a saved view is invalid", () =>
  {
    updateNavigationResourceState(
      "organization:organization-alpha",
      {
        lastLocalView: "overview",
      }
    );

    render(<ContextNavigation />);

    expect(
      screen.getByRole("link",
      {
        name: "Studio Alpha",
      })
    ).toHaveAttribute(
      "href",
      "/organizations/organization-alpha/all"
    );
  });

  it("renders an empty state when no Organizations exist", () =>
  {
    mockOrganizations([]);

    render(<ContextNavigation />);

    expect(
      screen.getByText("No workspaces yet.")
    ).toBeInTheDocument();

    expect(
      screen.queryByRole("link",
      {
        name: "Studio Alpha",
      })
    ).not.toBeInTheDocument();
  });

  it("renders a loading state", () =>
  {
    mockedUseOrganizations.mockReturnValue(
    {
      data: undefined,
      isPending: true,
      isError: false,
    } as ReturnType<typeof useOrganizations>);

    render(<ContextNavigation />);

    expect(
      screen.getByText("Loading workspaces...")
    ).toBeInTheDocument();

    expect(
      screen.queryByText("No workspaces yet.")
    ).not.toBeInTheDocument();
  });

  it("renders an error state", () =>
  {
    mockedUseOrganizations.mockReturnValue(
    {
      data: undefined,
      isPending: false,
      isError: true,
    } as ReturnType<typeof useOrganizations>);

    render(<ContextNavigation />);

    expect(
      screen.getByRole("alert")
    ).toHaveTextContent("Unable to load workspaces.");

    expect(
      screen.queryByText("No workspaces yet.")
    ).not.toBeInTheDocument();
  });

  it("expands an Organization without navigating", () =>
  {
    render(<ContextNavigation />);

    const toggle = screen.getByRole("button",
    {
      name: "Expand Studio Alpha",
    });

    expect(toggle).toHaveAttribute("aria-expanded", "false");

    fireEvent.click(toggle);

    expect(
      screen.getByRole("button",
      {
        name: "Collapse Studio Alpha",
      })
    ).toHaveAttribute("aria-expanded", "true");

    expect(
      screen.getByText("No projects yet.")
    ).toBeInTheDocument();

    expect(
      screen.getByRole("link",
      {
        name: "Home",
      })
    ).toHaveAttribute("aria-current", "location");
  });

  it("allows multiple Organizations to remain expanded", () =>
  {
    render(<ContextNavigation />);

    fireEvent.click(
      screen.getByRole("button",
      {
        name: "Expand Studio Alpha",
      })
    );

    fireEvent.click(
      screen.getByRole("button",
      {
        name: "Expand Studio Beta",
      })
    );

    expect(
      screen.getByRole("button",
      {
        name: "Collapse Studio Alpha",
      })
    ).toHaveAttribute("aria-expanded", "true");

    expect(
      screen.getByRole("button",
      {
        name: "Collapse Studio Beta",
      })
    ).toHaveAttribute("aria-expanded", "true");

    expect(
      screen.getAllByText("No projects yet.")
    ).toHaveLength(2);
  });

  it("collapses one Organization without affecting the others", () =>
  {
    render(<ContextNavigation />);

    fireEvent.click(
      screen.getByRole("button",
      {
        name: "Expand Studio Alpha",
      })
    );

    fireEvent.click(
      screen.getByRole("button",
      {
        name: "Expand Studio Beta",
      })
    );

    fireEvent.click(
      screen.getByRole("button",
      {
        name: "Collapse Studio Alpha",
      })
    );

    expect(
      screen.getByRole("button",
      {
        name: "Expand Studio Alpha",
      })
    ).toHaveAttribute("aria-expanded", "false");

    expect(
      screen.getByRole("button",
      {
        name: "Collapse Studio Beta",
      })
    ).toHaveAttribute("aria-expanded", "true");

    expect(
      screen.getAllByText("No projects yet.")
    ).toHaveLength(1);
  });

  it("automatically expands the parent Organization of a Project", () =>
  {
    mockedUsePathname.mockReturnValue(
      "/organizations/organization-beta/projects/project-one/all"
    );

    render(<ContextNavigation />);

    expect(
      screen.getByRole("button",
      {
        name: "Collapse Studio Beta",
      })
    ).toHaveAttribute("aria-expanded", "true");

    expect(
      screen.getByRole("button",
      {
        name: "Expand Studio Alpha",
      })
    ).toHaveAttribute("aria-expanded", "false");

    expect(
      screen.getByRole("link",
      {
        name: "Studio Beta",
      })
    ).toHaveAttribute("data-context-status", "current");
  });

  it("allows manually collapsing an automatically expanded Organization", () =>
  {
    mockedUsePathname.mockReturnValue(
      "/organizations/organization-beta/projects/project-one/all"
    );

    render(<ContextNavigation />);

    expect(
      screen.getByRole("button",
      {
        name: "Collapse Studio Beta",
      })
    ).toHaveAttribute("aria-expanded", "true");

    fireEvent.click(
      screen.getByRole("button",
      {
        name: "Collapse Studio Beta",
      })
    );

    expect(
      screen.getByRole("button",
      {
        name: "Expand Studio Beta",
      })
    ).toHaveAttribute("aria-expanded", "false");

    expect(
      screen.queryByText("No projects yet.")
    ).not.toBeInTheDocument();

    expect(
      screen.getByRole("link",
      {
        name: "Studio Beta",
      })
    ).toHaveAttribute("data-context-status", "current");
  });

  it("allows reopening an automatically expanded Organization after manual collapse", () =>
  {
    mockedUsePathname.mockReturnValue(
      "/organizations/organization-beta/projects/project-one/all"
    );

    render(<ContextNavigation />);

    fireEvent.click(
      screen.getByRole("button",
      {
        name: "Collapse Studio Beta",
      })
    );

    fireEvent.click(
      screen.getByRole("button",
      {
        name: "Expand Studio Beta",
      })
    );

    expect(
      screen.getByRole("button",
      {
        name: "Collapse Studio Beta",
      })
    ).toHaveAttribute("aria-expanded", "true");

    expect(
      screen.getByText("No projects yet.")
    ).toBeInTheDocument();
  });

  it("expands the destination Organization when navigation changes", () =>
  {
    const { rerender } = render(<ContextNavigation />);

    expect(
      screen.getByRole("button",
      {
        name: "Expand Studio Alpha",
      })
    ).toHaveAttribute("aria-expanded", "false");

    act(() =>
    {
      mockedUsePathname.mockReturnValue(
        "/organizations/organization-alpha/all"
      );
    });

    rerender(<ContextNavigation />);

    expect(
      screen.getByRole("button",
      {
        name: "Collapse Studio Alpha",
      })
    ).toHaveAttribute("aria-expanded", "true");
  });

  it("preserves other expansions when navigating between Organizations", () =>
  {
    const { rerender } = render(<ContextNavigation />);

    fireEvent.click(
      screen.getByRole("button",
      {
        name: "Expand Studio Alpha",
      })
    );

    mockedUsePathname.mockReturnValue(
      "/organizations/organization-beta/all"
    );

    rerender(<ContextNavigation />);

    expect(
      screen.getByRole("button",
      {
        name: "Collapse Studio Alpha",
      })
    ).toHaveAttribute("aria-expanded", "true");

    expect(
      screen.getByRole("button",
      {
        name: "Collapse Studio Beta",
      })
    ).toHaveAttribute("aria-expanded", "true");
  });

  it("filters Organizations without case sensitivity", () =>
  {
    render(<ContextNavigation />);

    fireEvent.change(
      screen.getByRole("searchbox",
      {
        name: "Filter workspaces",
      }),
      {
        target:
        {
          value: "ALPHA",
        },
      }
    );

    expect(
      screen.getByRole("link",
      {
        name: "Studio Alpha",
      })
    ).toBeInTheDocument();

    expect(
      screen.queryByRole("link",
      {
        name: "Studio Beta",
      })
    ).not.toBeInTheDocument();
  });

  it("shows an empty result when no Organization matches", () =>
  {
    render(<ContextNavigation />);

    fireEvent.change(
      screen.getByRole("searchbox",
      {
        name: "Filter workspaces",
      }),
      {
        target:
        {
          value: "Unknown",
        },
      }
    );

    expect(
      screen.getByText("No matching workspaces.")
    ).toBeInTheDocument();

    expect(
      screen.getByRole("link",
      {
        name: "Home",
      })
    ).toHaveAttribute("aria-current", "location");
  });

  it("restores the previous expansion after clearing the filter", () =>
  {
    render(<ContextNavigation />);

    fireEvent.click(
      screen.getByRole("button",
      {
        name: "Expand Studio Alpha",
      })
    );

    const filter = screen.getByRole("searchbox",
    {
      name: "Filter workspaces",
    });

    fireEvent.change(filter,
    {
      target:
      {
        value: "Beta",
      },
    });

    expect(
      screen.queryByRole("button",
      {
        name: "Collapse Studio Alpha",
      })
    ).not.toBeInTheDocument();

    fireEvent.change(filter,
    {
      target:
      {
        value: "",
      },
    });

    expect(
      screen.getByRole("button",
      {
        name: "Collapse Studio Alpha",
      })
    ).toHaveAttribute("aria-expanded", "true");
  });

  it("does not change the active Organization when filtering", () =>
  {
    mockedUsePathname.mockReturnValue(
      "/organizations/organization-alpha/all"
    );

    render(<ContextNavigation />);

    fireEvent.change(
      screen.getByRole("searchbox",
      {
        name: "Filter workspaces",
      }),
      {
        target:
        {
          value: "Beta",
        },
      }
    );

    expect(
      screen.getByRole("link",
      {
        name: "Studio Beta",
      })
    ).toHaveAttribute("data-context-status", "available");

    expect(
      screen.getByRole("link",
      {
        name: "Home",
      })
    ).not.toHaveAttribute("aria-current", "location");
  });
});
