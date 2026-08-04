import { resolveNavigationLocation } from "@/navigation/navigation-resolver";

describe("navigation resolver", () =>
{
  it("resolves the personal home location", () =>
  {
    expect(resolveNavigationLocation("/home")).toEqual(
    {
      pathname: "/home",
      contextKind: "home",
      contextKey: "home",
      presentationModel: "personal-hub",
      localView: null,
      hierarchy: [],
    });
  });

  it("resolves an Organization Explorer location", () =>
  {
    expect(
      resolveNavigationLocation(
        "/organizations/demo-organization/projects"
      )
    ).toEqual(
    {
      pathname: "/organizations/demo-organization/projects",
      contextKind: "organization",
      contextKey: "organization:demo-organization",
      presentationModel: "explorer",
      organizationId: "demo-organization",
      localView: "projects",
      hierarchy:
      [
        {
          kind: "organization",
          id: "demo-organization",
        },
      ],
    });
  });

  it("resolves a Project Explorer with its Organization parent", () =>
  {
    expect(
      resolveNavigationLocation(
        "/organizations/demo-organization/projects/demo-project/campaigns"
      )
    ).toEqual(
    {
      pathname:
        "/organizations/demo-organization/projects/demo-project/campaigns",
      contextKind: "project",
      contextKey: "project:demo-project",
      presentationModel: "explorer",
      organizationId: "demo-organization",
      projectId: "demo-project",
      localView: "campaigns",
      hierarchy:
      [
        {
          kind: "organization",
          id: "demo-organization",
        },
        {
          kind: "project",
          id: "demo-project",
        },
      ],
    });
  });

  it("resolves a Campaign Workspace with its complete hierarchy", () =>
  {
    expect(
      resolveNavigationLocation(
        "/organizations/demo-organization/projects/demo-project/campaigns/demo-campaign/results"
      )
    ).toEqual(
    {
      pathname:
        "/organizations/demo-organization/projects/demo-project/campaigns/demo-campaign/results",
      contextKind: "campaign",
      contextKey: "campaign:demo-campaign",
      presentationModel: "workspace",
      organizationId: "demo-organization",
      projectId: "demo-project",
      campaignId: "demo-campaign",
      localView: "results",
      hierarchy:
      [
        {
          kind: "organization",
          id: "demo-organization",
        },
        {
          kind: "project",
          id: "demo-project",
        },
        {
          kind: "campaign",
          id: "demo-campaign",
        },
      ],
    });
  });

  it("resolves a Participation Document outside the Organization hierarchy", () =>
  {
    expect(
      resolveNavigationLocation(
        "/participations/demo-participation/consent"
      )
    ).toEqual(
    {
      pathname: "/participations/demo-participation/consent",
      contextKind: "participation",
      contextKey: "participation:demo-participation",
      presentationModel: "document",
      participationId: "demo-participation",
      localView: "consent",
      hierarchy:
      [
        {
          kind: "participation",
          id: "demo-participation",
        },
      ],
    });
  });

  it("decodes identifiers and returns a canonical pathname", () =>
  {
    expect(
      resolveNavigationLocation(
        "/organizations/research%20europe/projects/"
      )
    ).toEqual(
    {
      pathname: "/organizations/research%20europe/projects",
      contextKind: "organization",
      contextKey: "organization:research europe",
      presentationModel: "explorer",
      organizationId: "research europe",
      localView: "projects",
      hierarchy:
      [
        {
          kind: "organization",
          id: "research europe",
        },
      ],
    });
  });

  it("rejects unsupported local views", () =>
  {
    expect(
      resolveNavigationLocation(
        "/organizations/demo-organization/dashboard"
      )
    ).toBeNull();

    expect(
      resolveNavigationLocation(
        "/participations/demo-participation/results"
      )
    ).toBeNull();
  });

  it("rejects incomplete or unrelated pathnames", () =>
  {
    expect(resolveNavigationLocation("/")).toBeNull();

    expect(
      resolveNavigationLocation(
        "/organizations/demo-organization/projects"
        + "/demo-project/campaigns/demo-campaign"
      )
    ).toBeNull();

    expect(resolveNavigationLocation("/settings")).toBeNull();
    expect(resolveNavigationLocation("home")).toBeNull();
  });

  it("rejects malformed encoded segments", () =>
  {
    expect(
      resolveNavigationLocation(
        "/organizations/%E0%A4%A/projects"
      )
    ).toBeNull();

    expect(
      resolveNavigationLocation(
        "/organizations//projects"
      )
    ).toBeNull();
  });
});
