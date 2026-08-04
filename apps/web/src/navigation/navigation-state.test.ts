import
{
  NAVIGATION_STATE_STORAGE_KEY,
  getPersistedLocalView,
  readNavigationState,
  resetNavigationState,
  updateNavigationResourceState,
  getNavigationScrollPosition,
  updateNavigationScrollPosition,
} from "@/navigation/navigation-state";

import
{
  PROJECT_VIEWS,
} from "@/navigation/navigation-types";

describe("navigation state", () =>
{
  beforeEach(() =>
  {
    resetNavigationState();
  });

  it("persists independent state for each resource", () =>
  {
    updateNavigationResourceState(
      "organization:demo-organization",
      {
        lastLocalView: "projects",
        explorerSearchQuery: "research",
      }
    );

    updateNavigationResourceState(
      "project:demo-project",
      {
        lastLocalView: "campaigns",
        explorerSearchQuery: "onboarding",
      }
    );

    const state = readNavigationState();

    expect(
      state.resources["organization:demo-organization"]
    ).toEqual(
    {
      lastLocalView: "projects",
      explorerSearchQuery: "research",
    });

    expect(
      state.resources["project:demo-project"]
    ).toEqual(
    {
      lastLocalView: "campaigns",
      explorerSearchQuery: "onboarding",
    });

    expect(
      JSON.parse(
        window.localStorage.getItem(
          NAVIGATION_STATE_STORAGE_KEY
        ) ?? ""
      )
    ).toEqual(state);
  });

  it("returns only a stored local view allowed by the resource", () =>
  {
    updateNavigationResourceState(
      "project:demo-project",
      {
        lastLocalView: "campaigns",
      }
    );

    expect(
      getPersistedLocalView(
        readNavigationState(),
        "project:demo-project",
        PROJECT_VIEWS,
        "all"
      )
    ).toBe("campaigns");

    updateNavigationResourceState(
      "project:demo-project",
      {
        lastLocalView: "unsupported-view",
      }
    );

    expect(
      getPersistedLocalView(
        readNavigationState(),
        "project:demo-project",
        PROJECT_VIEWS,
        "all"
      )
    ).toBe("all");
  });

  it("persists independent scroll positions by view", () =>
  {
    updateNavigationScrollPosition(
      "campaign:demo-campaign",
      "results",
      320
    );

    updateNavigationScrollPosition(
      "campaign:demo-campaign",
      "settings",
      80
    );

    const state = readNavigationState();

    expect(
      getNavigationScrollPosition(
        state,
        "campaign:demo-campaign",
        "results"
      )
    ).toBe(320);

    expect(
      getNavigationScrollPosition(
        state,
        "campaign:demo-campaign",
        "settings"
      )
    ).toBe(80);
  });
});
