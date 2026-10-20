import
{
  uploadAvatarFile,
} from "@/features/current-user-profile/upload-avatar-file";

describe("uploadAvatarFile", () =>
{
  beforeEach(() =>
  {
    global.fetch = jest.fn();
  });

  afterEach(() =>
  {
    jest.restoreAllMocks();
  });

  it("uploads the avatar file to the presigned URL", async () =>
  {
    jest.mocked(global.fetch).mockResolvedValue(
    {
      ok: true,
      status: 200,
    } as Response);

    const file = new File(
      ["avatar-content"],
      "avatar.png",
      {
        type: "image/png",
      },
    );

    const abortController = new AbortController();

    await uploadAvatarFile(
      "https://storage.example.test/upload",
      file,
      abortController.signal,
    );

    expect(global.fetch).toHaveBeenCalledWith(
      "https://storage.example.test/upload",
      {
        method: "PUT",
        headers:
        {
          "Content-Type": "image/png",
        },
        body: file,
        signal: abortController.signal,
      },
    );
  });

  it("throws when the object storage upload fails", async () =>
  {
    jest.mocked(global.fetch).mockResolvedValue(
    {
      ok: false,
      status: 500,
    } as Response);

    const file = new File(
      ["avatar-content"],
      "avatar.png",
      {
        type: "image/png",
      },
    );

    await expect(
      uploadAvatarFile(
        "https://storage.example.test/upload",
        file,
      )
    ).rejects.toThrow(
      "Avatar upload failed with status 500.",
    );
  });
});