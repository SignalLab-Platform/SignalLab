import
{
  fireEvent,
  render,
  screen,
  waitFor,
} from "@testing-library/react";

import { AppContent } from "@/components/shell/app-content";

let mockPathname =
  "/organizations/demo-organization/projects/demo-project/campaigns";

let mockSearchParams =
  new URLSearchParams();

const mockPush = jest.fn();
const mockBack = jest.fn();
const mockReplace = jest.fn();

jest.mock("next/navigation", () =>
{
  return {
    usePathname: () =>
      mockPathname,

    useSearchParams: () =>
      mockSearchParams,

    useRouter: () =>
    {
      return {
        push: mockPush,
        back: mockBack,
        replace: mockReplace,
      };
    },
  };
});

function renderController()
{
  return render(
    <AppContent>
      <p>Underlying Project</p>
    </AppContent>
  );
}

describe("AnalysisOverlayController", () =>
{
  beforeEach(() =>
  {
    mockPathname =
      "/organizations/demo-organization/projects/demo-project/campaigns";

    mockSearchParams =
      new URLSearchParams();

    mockPush.mockClear();
    mockBack.mockClear();
    mockReplace.mockClear();
  });

  it("opens Analytics through the URL without changing the pathname", () =>
  {
    renderController();

    fireEvent.click(
      screen.getByRole(
        "button",
        {
          name: "Analytics",
        }
      )
    );

    expect(mockPush).toHaveBeenCalledWith(
      "/organizations/demo-organization/projects/demo-project/campaigns?overlay=analysis",
      {
        scroll: false,
      }
    );
  });

  it("renders the Analysis Overlay structure from the URL", async () =>
  {
    mockSearchParams =
      new URLSearchParams(
        "overlay=analysis"
      );

    renderController();

    expect(
      screen.getByRole(
        "dialog",
        {
          name: "New Analysis",
        }
      )
    ).toBeInTheDocument();

    expect(
      screen.getByRole(
        "navigation",
        {
          name: "Analysis navigation",
        }
      )
    ).toBeInTheDocument();

    expect(
      screen.getByRole(
        "tab",
        {
          name: "Subjects",
        }
      )
    ).toHaveAttribute(
      "aria-selected",
      "true"
    );

    expect(
      screen.getByRole(
        "tab",
        {
          name: "Sources",
        }
      )
    ).toBeInTheDocument();

    const closeButton =
      screen.getByRole(
        "button",
        {
          name: "Close Analysis",
        }
      );

    await waitFor(() =>
    {
      expect(closeButton).toHaveFocus();
    });
  });

  it("uses browser history when closed after the floating action", () =>
  {
    const result = renderController();

    fireEvent.click(
      screen.getByRole(
        "button",
        {
          name: "Analytics",
        }
      )
    );

    mockSearchParams =
      new URLSearchParams(
        "overlay=analysis"
      );

    result.rerender(
      <AppContent>
        <p>Underlying Project</p>
      </AppContent>
    );

    fireEvent.click(
      screen.getByRole(
        "button",
        {
          name: "Close Analysis",
        }
      )
    );

    expect(
      mockBack
    ).toHaveBeenCalledTimes(1);
  });

  it("removes the query parameter when opened directly", () =>
  {
    mockSearchParams =
      new URLSearchParams(
        "filter=active&overlay=analysis"
      );

    renderController();

    fireEvent.click(
      screen.getByRole(
        "button",
        {
          name: "Close Analysis",
        }
      )
    );

    expect(
      mockReplace
    ).toHaveBeenCalledWith(
      "/organizations/demo-organization/projects/demo-project/campaigns?filter=active",
      {
        scroll: false,
      }
    );
  });

  it("closes the directly opened Overlay with Escape", () =>
  {
    mockSearchParams =
      new URLSearchParams(
        "overlay=analysis"
      );

    renderController();

    fireEvent.keyDown(
      document,
      {
        key: "Escape",
      }
    );

    expect(
      mockReplace
    ).toHaveBeenCalledWith(
      "/organizations/demo-organization/projects/demo-project/campaigns",
      {
        scroll: false,
      }
    );
  });

  it("closes the directly opened Overlay through its backdrop", () =>
  {
    mockSearchParams =
      new URLSearchParams(
        "overlay=analysis"
      );

    renderController();

    const backdrop =
      document.querySelector<HTMLElement>(
        "[data-analysis-overlay-backdrop]"
      );

    if (backdrop === null)
    {
      throw new Error(
        "Unable to find the Analysis backdrop."
      );
    }

    fireEvent.mouseDown(backdrop);

    expect(
      mockReplace
    ).toHaveBeenCalledWith(
      "/organizations/demo-organization/projects/demo-project/campaigns",
      {
        scroll: false,
      }
    );
  });

  it("does not expose Analytics outside an eligible context", () =>
  {
    mockPathname = "/home";

    renderController();

    expect(
      screen.queryByRole(
        "button",
        {
          name: "Analytics",
        }
      )
    ).not.toBeInTheDocument();
  });
});
