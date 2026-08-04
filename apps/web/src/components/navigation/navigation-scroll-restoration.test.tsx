import
{
  fireEvent,
  render,
  waitFor,
} from "@testing-library/react";

import
{
  NavigationScrollRestoration,
  getNavigationScrollKey,
} from "@/components/navigation/navigation-scroll-restoration";

jest.mock(
  "@/components/analysis/analysis-overlay-controller",
  () =>
  {
    return {
      AnalysisOverlayController: () => null,
    };
  }
);

import { AppContent } from "@/components/shell/app-content";

import
{
  getNavigationScrollPosition,
  readNavigationState,
  resetNavigationState,
  updateNavigationScrollPosition,
} from "@/navigation/navigation-state";

import
{
  resolveNavigationLocation,
} from "@/navigation/navigation-resolver";

import type
{
  NavigationLocation,
} from "@/navigation/navigation-types";

function resolveRequiredLocation(
  pathname: string
): NavigationLocation
{
  const location = resolveNavigationLocation(pathname);

  if (location === null)
  {
    throw new Error(
      `Unable to resolve test pathname: ${pathname}`
    );
  }

  return location;
}

function renderScrollRestoration(
  location: NavigationLocation
)
{
  const result = render(
    <AppContent>
      <NavigationScrollRestoration
        location={location}
      />

      <div className="h-[2000px]" />
    </AppContent>
  );

  const scrollport =
    result.container.querySelector<HTMLElement>(
      "#application-content-scrollport"
    );

  if (scrollport === null)
  {
    throw new Error(
      "Unable to find the application Content scrollport."
    );
  }

  return {
    ...result,
    scrollport,
  };
}

describe("NavigationScrollRestoration", () =>
{
  beforeEach(() =>
  {
    resetNavigationState();

    window.history.replaceState(
      null,
      "",
      "/"
    );
  });

  afterEach(() =>
  {
    window.history.replaceState(
      null,
      "",
      "/"
    );
  });

  it("restores the stored scroll for the current view", () =>
  {
    const location = resolveRequiredLocation(
      "/organizations/demo-organization/projects/demo-project/campaigns/demo-campaign/results"
    );

    updateNavigationScrollPosition(
      location.contextKey,
      "results",
      240
    );

    const { scrollport } =
      renderScrollRestoration(location);

    expect(scrollport.scrollTop).toBe(240);
  });

  it("persists scroll changes for the current view", async () =>
  {
    const location = resolveRequiredLocation(
      "/organizations/demo-organization/projects/demo-project/campaigns/demo-campaign/settings"
    );

    const { scrollport } =
      renderScrollRestoration(location);

    scrollport.scrollTop = 420;

    fireEvent.scroll(scrollport);

    await waitFor(() =>
    {
      expect(
        getNavigationScrollPosition(
          readNavigationState(),
          location.contextKey,
          "settings"
        )
      ).toBe(420);
    });
  });

  it("uses independent keys for Workspace views", () =>
  {
    const resultsLocation = resolveRequiredLocation(
      "/organizations/demo-organization/projects/demo-project/campaigns/demo-campaign/results"
    );

    const settingsLocation = resolveRequiredLocation(
      "/organizations/demo-organization/projects/demo-project/campaigns/demo-campaign/settings"
    );

    expect(
      getNavigationScrollKey(resultsLocation)
    ).toBe("results");

    expect(
      getNavigationScrollKey(settingsLocation)
    ).toBe("settings");
  });

  it("uses one scroll key for the continuous Participation document", () =>
  {
    const consentLocation = resolveRequiredLocation(
      "/participations/demo-participation/consent"
    );

    const supportLocation = resolveRequiredLocation(
      "/participations/demo-participation/support"
    );

    expect(
      getNavigationScrollKey(consentLocation)
    ).toBe("document");

    expect(
      getNavigationScrollKey(supportLocation)
    ).toBe("document");
  });
});
