import
{
  completeAvatarUpload,
} from "@/features/current-user-profile/complete-avatar-upload";

import
{
  createAvatarUpload,
} from "@/features/current-user-profile/create-avatar-upload";

import
{
  uploadAvatarFile,
} from "@/features/current-user-profile/upload-avatar-file";

type ChangeCurrentUserAvatarOptions = Readonly<
{
  token: string;
  file: File;
  signal?: AbortSignal;
}>;

export async function changeCurrentUserAvatar(
  options: ChangeCurrentUserAvatarOptions,
)
{
  const upload = await createAvatarUpload(
  {
    token: options.token,
    file: options.file,
    signal: options.signal,
  });

  await uploadAvatarFile(
    upload.uploadUrl,
    options.file,
    options.signal,
  );

  await completeAvatarUpload(
  {
    token: options.token,
    fileId: upload.fileId,
    signal: options.signal,
  });
}