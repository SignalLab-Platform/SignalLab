import
{
  render,
  waitFor,
} from "@testing-library/react";

import
{
  NavigationStateRecorder,
} from "@/components/navigation/navigation-state-recorder";

import
{
  readNavigationState,
  resetNavigationState,
} from "@/navigation/navigation-state";

import
{
  resolveNavigationLocation,
} from "@/navigation/navigation-resolver";

describe("NavigationStateRecorder", () =>
{
  beforeEach(() =>
  {
    resetNavigationState();
  });

  it("records the current local view for its resource", async () =>
  {
    const location = resolveNavigationLocation(
      "/organizations/demo-organization/projects/demo-project/campaigns/demo-campaign/results"
    );

    if (location === null)
    {
      throw new Error("Unable to resolve the Campaign test route.");
    }

    render(
      <NavigationStateRecorder location={location} />
    );

    await waitFor(() =>
    {
      expect(
        readNavigationState()
          .resources["campaign:demo-campaign"]
          ?.lastLocalView
      ).toBe("results");
    });
  });
});
