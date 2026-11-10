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
      presentationModel: "object-explorer",
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
      presentationModel: "object-explorer",
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
      presentationModel: "object-explorer",
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

  it("does not resolve a standalone Organizations PersonalExplorer", () =>
  {
    expect(resolveNavigationLocation("/organizations")).toBeNull();
  });

  describe("Organization Administration navigation", () =>
  {
    it.each([
      "members",
      "invitations",
      "settings",
    ])("resolves the %s administration view", (view) =>
    {
      const location = resolveNavigationLocation(
        `/organizations/demo-organization/administration/${view}`
      );

      expect(location).toMatchObject({
        contextKind: "organization-administration",
        contextKey: "organization-administration:demo-organization",
        presentationModel: "organization-administration",
        organizationId: "demo-organization",
        localView: view,
        hierarchy:
        [
          {
            kind: "organization",
            id: "demo-organization",
          },
        ],
      });
    });

    it("rejects unknown administration views", () =>
    {
      expect(
        resolveNavigationLocation(
          "/organizations/demo-organization/administration/unknown"
        )
      ).toBeNull();
    });

    it("keeps Explorer and Administration contexts distinct", () =>
    {
      const explorer = resolveNavigationLocation(
        "/organizations/demo-organization/projects"
      );

      const administration = resolveNavigationLocation(
        "/organizations/demo-organization/administration/members"
      );

      expect(explorer?.contextKey).not.toBe(
        administration?.contextKey
      );
    });
  });

  it("resolves an Organization Invitation as an independent context", () =>
  {
    expect(
      resolveNavigationLocation(
        "/organization-invitations/demo-invitation"
      )
    ).toEqual(
    {
      pathname: "/organization-invitations/demo-invitation",
      contextKind: "organization-invitation",
      contextKey: "organization-invitation:demo-invitation",
      presentationModel: "document",
      invitationId: "demo-invitation",
      localView: null,
      hierarchy: [],
    });
  });

  it("normalizes an Organization Invitation pathname", () =>
  {
    expect(
      resolveNavigationLocation(
        "/organization-invitations/demo%20invitation/?from=%2Fhome"
      )
    ).toEqual(
    {
      pathname: "/organization-invitations/demo%20invitation",
      contextKind: "organization-invitation",
      contextKey: "organization-invitation:demo invitation",
      presentationModel: "document",
      invitationId: "demo invitation",
      localView: null,
      hierarchy: [],
    });
  });

  it("rejects incomplete Organization Invitation routes", () =>
  {
    expect(
      resolveNavigationLocation("/organization-invitations")
    ).toBeNull();

    expect(
      resolveNavigationLocation(
        "/organization-invitations/demo-invitation/accept"
      )
    ).toBeNull();
  });
});
