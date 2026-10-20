import { apiPost } from "@/lib/api/api-client";

type CompleteAvatarUploadOptions = Readonly<
{
  token: string;
  fileId: string;
  signal?: AbortSignal;
}>;

export async function completeAvatarUpload(
  options: CompleteAvatarUploadOptions,
)
{
  await apiPost(
    "/users/me/avatar/complete",
    {
      fileId: options.fileId,
    },
    {
      token: options.token,
      signal: options.signal,
    },
  );
}