import {
  buildOrganizationInvitationRoute,
  getSafeInvitationOrigin,
} from "./invitation-navigation";

describe("Invitation navigation", () =>
{
  it("builds a canonical invitation link without an origin", () =>
  {
    expect(
      buildOrganizationInvitationRoute("invitation-123")
    ).toBe(
      "/organization-invitations/invitation-123"
    );
  });

  it("preserves a valid local origin", () =>
  {
    const route = buildOrganizationInvitationRoute(
      "invitation-123",
      "/home"
    );

    expect(route).toBe(
      "/organization-invitations/invitation-123?from=%2Fhome"
    );
  });

  it("preserves local query parameters", () =>
  {
    expect(
      getSafeInvitationOrigin("/home?tab=organizations")
    ).toBe("/home?tab=organizations");
  });

  it.each(
  [
    "https://example.com",
    "https://example.com/evil",
    "//example.com/evil",
    "/\\example.com/evil",
    "javascript:alert(1)",
    "/organization-invitations/another-invitation",
    "",
  ])(
    "rejects an unsafe origin: %s",
    (origin) =>
    {
      expect(getSafeInvitationOrigin(origin)).toBeNull();
    }
  );

  it("ignores an unsafe origin when building a link", () =>
  {
    expect(
      buildOrganizationInvitationRoute(
        "invitation-123",
        "https://example.com"
      )
    ).toBe(
      "/organization-invitations/invitation-123"
    );
  });
});