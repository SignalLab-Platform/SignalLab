
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import { OrganizationsPersonalExplorer } from "./organizations-personal-explorer";
import { useOrganizations } from "./use-organizations";

jest.mock("./use-organizations", () =>
{
  return {
    useOrganizations: jest.fn(),
  };
});

jest.mock("./create-organization-dialog", () =>
{
  return {
    CreateOrganizationDialog: (
      {
        open,
        onOpenChange,
      }: {
        open: boolean;
        onOpenChange: (open: boolean) => void;
      }
    ) =>
    {
      if (!open)
      {
        return null;
      }

      return (
        <div role="dialog" aria-label="Create Organization">
          <button
            type="button"
            onClick={() =>
            {
              onOpenChange(false);
            }}
          >
            Close dialog
          </button>
        </div>
      );
    },
  };
});

const mockUseOrganizations = jest.mocked(useOrganizations);

function setupOrganizations(
  overrides: Record<string, unknown> = {}
)
{
  mockUseOrganizations.mockReturnValue(
  {
    data: [],
    isPending: false,
    isError: false,
    ...overrides,
  } as unknown as ReturnType<typeof useOrganizations>);
}

describe("OrganizationsPersonalExplorer", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();
    setupOrganizations();
  });

  it("displays the Create Organization card", () =>
  {
    render(<OrganizationsPersonalExplorer />);

    expect(
      screen.getByRole("button", { name: "Create Organization" })
    ).toBeEnabled();

    expect(
      screen.queryByRole("dialog")
    ).not.toBeInTheDocument();
  });

  it("opens the creation dialog when the card is clicked", async () =>
  {
    const user = userEvent.setup();

    render(<OrganizationsPersonalExplorer />);

    await user.click(
      screen.getByRole("button", { name: "Create Organization" })
    );

    expect(
      screen.getByRole("dialog", { name: "Create Organization" })
    ).toBeInTheDocument();
  });

  it("closes the creation dialog when requested", async () =>
  {
    const user = userEvent.setup();

    render(<OrganizationsPersonalExplorer />);

    await user.click(
      screen.getByRole("button", { name: "Create Organization" })
    );

    await user.click(
      screen.getByRole("button", { name: "Close dialog" })
    );

    expect(
      screen.queryByRole("dialog")
    ).not.toBeInTheDocument();
  });

  it("displays organizations returned by the query", () =>
  {
    setupOrganizations(
    {
      data:
      [
        {
          id: "organization-1",
          name: "SignalLab Studio",
          country: "FR",
          website: null,
        },
        {
          id: "organization-2",
          name: "Another Studio",
          country: "DE",
          website: null,
        },
      ],
    });

    render(<OrganizationsPersonalExplorer />);

    expect(
      screen.getByRole("link", { name: /SignalLab Studio/i })
    ).toHaveAttribute("href", "/organizations/organization-1/all");

    expect(
      screen.getByRole("link", { name: /Another Studio/i })
    ).toHaveAttribute("href", "/organizations/organization-2/all");
  });

  it("displays loading placeholders while organizations are loading", () =>
  {
    setupOrganizations(
    {
      isPending: true,
    });

    const { container } = render(<OrganizationsPersonalExplorer />);

    expect(
      container.querySelectorAll(".animate-pulse")
    ).toHaveLength(2);
  });

  it("displays an error message when loading fails", () =>
  {
    setupOrganizations(
    {
      isError: true,
    });

    render(<OrganizationsPersonalExplorer />);

    expect(
      screen.getByRole("alert")
    ).toHaveTextContent("We couldn't load your organizations.");
  });
});
