import { apiPost } from "@/lib/api/api-client";

import
{
  createAvatarUpload,
} from "@/features/current-user-profile/create-avatar-upload";

jest.mock("@/lib/api/api-client");

describe("createAvatarUpload", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();
  });

  it("creates an avatar upload from the selected file", async () =>
  {
    const file = new File(
      ["avatar-content"],
      "avatar.png",
      {
        type: "image/png",
      },
    );

    const upload =
    {
      fileId: "7e34f695-46df-4c29-9852-030783798531",
      uploadUrl: "https://storage.example.test/upload",
      expiresAt: "2026-10-04T20:00:00Z",
    };

    const response =
    {
      json: jest.fn().mockResolvedValue(upload),
    } as unknown as Response;

    jest.mocked(apiPost).mockResolvedValue(response);

    const abortController = new AbortController();

    const result = await createAvatarUpload(
    {
      token: "test-token",
      file,
      signal: abortController.signal,
    });

    expect(apiPost).toHaveBeenCalledWith(
      "/users/me/avatar/upload",
      {
        fileName: "avatar.png",
        contentType: "image/png",
        size: file.size,
      },
      {
        token: "test-token",
        signal: abortController.signal,
      },
    );

    expect(response.json).toHaveBeenCalledTimes(1);
    expect(result).toEqual(upload);
  });
});