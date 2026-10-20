import { apiPost } from "@/lib/api/api-client";

export type CreateAvatarUploadResult =
{
  fileId: string;
  uploadUrl: string;
  expiresAt: string;
};

type CreateAvatarUploadOptions = Readonly<
{
  token: string;
  file: File;
  signal?: AbortSignal;
}>;

export async function createAvatarUpload(
  options: CreateAvatarUploadOptions,
): Promise<CreateAvatarUploadResult>
{
  const response = await apiPost(
    "/users/me/avatar/upload",
    {
      fileName: options.file.name,
      contentType: options.file.type,
      size: options.file.size,
    },
    {
      token: options.token,
      signal: options.signal,
    },
  );

  return response.json() as Promise<CreateAvatarUploadResult>;
}