export type CurrentUserProfile =
{
  id: string;
  email: string;
  firstName: string | null;
  lastName: string | null;
  language: string;
  avatarFileId: string | null;
  avatarUrl: string | null;
};