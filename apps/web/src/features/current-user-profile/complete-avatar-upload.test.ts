import { apiPost } from "@/lib/api/api-client";

import
{
  completeAvatarUpload,
} from "@/features/current-user-profile/complete-avatar-upload";

jest.mock("@/lib/api/api-client");

describe("completeAvatarUpload", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();
  });

  it("completes the avatar upload", async () =>
  {
    jest.mocked(apiPost).mockResolvedValue(
    {
      ok: true,
      status: 204,
    } as Response);

    const abortController = new AbortController();

    await completeAvatarUpload(
    {
      token: "test-token",
      fileId: "7e34f695-46df-4c29-9852-030783798531",
      signal: abortController.signal,
    });

    expect(apiPost).toHaveBeenCalledWith(
      "/users/me/avatar/complete",
      {
        fileId: "7e34f695-46df-4c29-9852-030783798531",
      },
      {
        token: "test-token",
        signal: abortController.signal,
      },
    );
  });
});