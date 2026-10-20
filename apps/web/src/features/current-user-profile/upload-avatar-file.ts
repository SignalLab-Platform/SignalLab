export async function uploadAvatarFile(
  uploadUrl: string,
  file: File,
  signal?: AbortSignal,
)
{
  const response = await fetch(
    uploadUrl,
    {
      method: "PUT",
      headers:
      {
        "Content-Type": file.type,
      },
      body: file,
      signal,
    },
  );

  if (!response.ok)
  {
    throw new Error(
      `Avatar upload failed with status ${response.status}.`,
    );
  }
}