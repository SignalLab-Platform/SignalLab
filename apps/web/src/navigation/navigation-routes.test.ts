import
{
  ANALYSIS_OVERLAY_QUERY_KEY,
  ANALYSIS_OVERLAY_QUERY_VALUE,
  HOME_ROUTE,
  buildAnalysisOverlayRoute,
  buildCampaignRoute,
  buildOrganizationRoute,
  buildParticipationRoute,
  buildProjectRoute,
  removeAnalysisOverlayFromRoute,
} from "@/navigation/navigation-routes";

describe("navigation routes", () =>
{
  it("provides the canonical home route", () =>
  {
    expect(HOME_ROUTE).toBe("/home");
  });

  it("builds the complete hierarchical context routes", () =>
  {
    expect(
      buildOrganizationRoute("demo-organization", "all")
    ).toBe("/organizations/demo-organization/all");

    expect(
      buildProjectRoute(
        "demo-organization",
        "demo-project",
        "campaigns"
      )
    ).toBe(
      "/organizations/demo-organization/projects/demo-project/campaigns"
    );

    expect(
      buildCampaignRoute(
        "demo-organization",
        "demo-project",
        "demo-campaign",
        "overview"
      )
    ).toBe(
      "/organizations/demo-organization/projects/demo-project/campaigns/demo-campaign/overview"
    );
  });

  it("builds participation routes outside the organization hierarchy", () =>
  {
    expect(
      buildParticipationRoute("demo-participation", "consent")
    ).toBe("/participations/demo-participation/consent");
  });

  it("encodes resource identifiers as individual URL segments", () =>
  {
    expect(
      buildOrganizationRoute("research europe", "projects")
    ).toBe("/organizations/research%20europe/projects");
  });

  it("rejects empty resource identifiers", () =>
  {
    expect(() =>
    {
      buildProjectRoute("demo-organization", "   ", "all");
    }).toThrow("projectId cannot be empty.");
  });

  it("opens the Analysis Overlay without changing the pathname", () =>
  {
    expect(
      buildAnalysisOverlayRoute(
        "/organizations/demo-organization/projects/demo-project/campaigns"
      )
    ).toBe(
      `/organizations/demo-organization/projects/demo-project/campaigns?${ANALYSIS_OVERLAY_QUERY_KEY}=${ANALYSIS_OVERLAY_QUERY_VALUE}`
    );
  });

  it("preserves existing query parameters and does not duplicate the overlay", () =>
  {
    const initialRoute = "/home?source=sidebar";

    const openedOnce = buildAnalysisOverlayRoute(initialRoute);
    const openedTwice = buildAnalysisOverlayRoute(openedOnce);

    expect(openedOnce).toBe("/home?source=sidebar&overlay=analysis");
    expect(openedTwice).toBe(openedOnce);
  });

  it("removes only the Analysis Overlay parameter", () =>
  {
    expect(
      removeAnalysisOverlayFromRoute(
        "/home?source=sidebar&overlay=analysis"
      )
    ).toBe("/home?source=sidebar");
  });

  it("adds the Analysis Overlay without replacing existing query parameters", () =>
  {
    expect(
      buildAnalysisOverlayRoute(
        "/organizations/demo-organization/projects/demo-project/campaigns?filter=active"
      )
    ).toBe(
      "/organizations/demo-organization/projects/demo-project/campaigns?filter=active&overlay=analysis"
    );
  });

  it("removes only the Analysis Overlay query parameter", () =>
  {
    expect(
      removeAnalysisOverlayFromRoute(
        "/organizations/demo-organization/projects/demo-project/campaigns?filter=active&overlay=analysis"
      )
    ).toBe(
      "/organizations/demo-organization/projects/demo-project/campaigns?filter=active"
    );
  });
});
