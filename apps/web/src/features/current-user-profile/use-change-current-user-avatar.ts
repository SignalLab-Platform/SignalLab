"use client";

import { useAuth } from "@clerk/nextjs";
import
{
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";

import
{
  changeCurrentUserAvatar,
} from "@/features/current-user-profile/change-current-user-avatar";
import
{
  currentUserProfileQueryKey,
} from "@/features/current-user-profile/use-current-user-profile";

export function useChangeCurrentUserAvatar()
{
  const { getToken } = useAuth();
  const queryClient = useQueryClient();

  return useMutation(
  {
    mutationFn: async (file: File) =>
    {
      const token = await getToken();

      if (!token)
      {
        throw new Error(
          "Unable to obtain an authentication token for the current user profile.",
        );
      }

      await changeCurrentUserAvatar(
      {
        token,
        file,
      });
    },
    onSuccess: async () =>
    {
      await queryClient.invalidateQueries(
      {
        queryKey: currentUserProfileQueryKey,
      });
    },
  });
}