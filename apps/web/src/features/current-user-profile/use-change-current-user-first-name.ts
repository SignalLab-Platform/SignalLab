"use client";

import { useAuth } from "@clerk/nextjs";
import
{
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";

import
{
  changeCurrentUserFirstName,
} from "@/features/current-user-profile/change-current-user-first-name";
import
{
  currentUserProfileQueryKey,
} from "@/features/current-user-profile/use-current-user-profile";

export function useChangeCurrentUserFirstName()
{
  const { getToken } = useAuth();
  const queryClient = useQueryClient();

  return useMutation(
  {
    mutationFn: async (firstName: string | null) =>
    {
      const token = await getToken();

      if (!token)
      {
        throw new Error(
          "Unable to obtain an authentication token for the current user profile.",
        );
      }

      return changeCurrentUserFirstName(
      {
        token,
        firstName,
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