import { apiGet, apiPost } from "@/lib/api/api-client";

import { getOrganizationInvitations } from "./get-organization-invitations";
import { getOrganizationInvitation } from "./get-organization-invitation";
import { getCurrentUserInvitations } from "./get-current-user-invitations";
import { createOrganizationInvitation } from "./create-organization-invitation";
import { acceptOrganizationInvitation } from "./accept-organization-invitation";
import { declineOrganizationInvitation } from "./decline-organization-invitation";
import { cancelOrganizationInvitation } from "./cancel-organization-invitation";

import { getCurrentUserNotifications } from "../notifications/get-current-user-notifications";
import { getUnreadNotificationCount } from "../notifications/get-unread-notification-count";
import { markNotificationAsRead } from "../notifications/mark-notification-as-read";

jest.mock("@/lib/api/api-client");

const mockedApiGet = jest.mocked(apiGet);
const mockedApiPost = jest.mocked(apiPost);

const token = "test-token";
const organizationId = "organization-1";
const invitationId = "invitation-1";

beforeEach(() =>
{
  jest.clearAllMocks();

  const response =
  {
    json: jest.fn().mockResolvedValue({ id: invitationId }),
  } as unknown as Response;

  mockedApiGet.mockResolvedValue(response);
  mockedApiPost.mockResolvedValue(response);
});

describe("Organization invitation API", () =>
{
  it("retrieves organization invitations", async () =>
  {
    await getOrganizationInvitations({ organizationId, token });

    expect(mockedApiGet).toHaveBeenCalledWith(
      `/organizations/${organizationId}/invitations`,
      { token, signal: undefined }
    );
  });

  it("retrieves a specific invitation", async () =>
  {
    await getOrganizationInvitation({ invitationId, token });

    expect(mockedApiGet).toHaveBeenCalledWith(
      `/organization-invitations/${invitationId}`,
      { token, signal: undefined }
    );
  });

  it("retrieves received invitations", async () =>
  {
    await getCurrentUserInvitations({ token });

    expect(mockedApiGet).toHaveBeenCalledWith(
      "/users/me/organization-invitations",
      { token, signal: undefined }
    );
  });

  it("creates an invitation using an email", async () =>
  {
    const input =
    {
      email: "user@example.com",
      roleId: "role-1",
      duration: 7 as const,
    };

    await createOrganizationInvitation({
      organizationId,
      input,
      token,
    });

    expect(mockedApiPost).toHaveBeenCalledWith(
      `/organizations/${organizationId}/invitations`,
      input,
      { token, signal: undefined }
    );
  });

  it.each(["accept", "decline", "cancel"] as const)(
    "sends a bodyless %s request",
    async (action) =>
    {
      const options = { invitationId, token };

      if (action === "accept")
      {
        await acceptOrganizationInvitation(options);
      }
      else if (action === "decline")
      {
        await declineOrganizationInvitation(options);
      }
      else
      {
        await cancelOrganizationInvitation(options);
      }

      expect(mockedApiPost).toHaveBeenCalledWith(
        `/organization-invitations/${invitationId}/${action}`,
        undefined,
        { token, signal: undefined }
      );
    }
  );
});

describe("Notification API", () =>
{
  it("retrieves recent notifications", async () =>
  {
    await getCurrentUserNotifications({ token });

    expect(mockedApiGet).toHaveBeenCalledWith(
      "/users/me/notifications",
      { token, signal: undefined }
    );
  });

  it("retrieves the unread count", async () =>
  {
    await getUnreadNotificationCount({ token });

    expect(mockedApiGet).toHaveBeenCalledWith(
      "/users/me/notifications/unread-count",
      { token, signal: undefined }
    );
  });

  it("marks a notification as read", async () =>
  {
    await markNotificationAsRead({
      notificationId: "notification-1",
      token,
    });

    expect(mockedApiPost).toHaveBeenCalledWith(
      "/users/me/notifications/notification-1/read",
      undefined,
      { token, signal: undefined }
    );
  });
});