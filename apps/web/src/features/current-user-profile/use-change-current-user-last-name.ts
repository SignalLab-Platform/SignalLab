"use client";

import { useAuth } from "@clerk/nextjs";
import
{
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";

import
{
  changeCurrentUserLastName,
} from "@/features/current-user-profile/change-current-user-last-name";
import
{
  currentUserProfileQueryKey,
} from "@/features/current-user-profile/use-current-user-profile";

export function useChangeCurrentUserLastName()
{
  const { getToken } = useAuth();
  const queryClient = useQueryClient();

  return useMutation(
  {
    mutationFn: async (lastName: string | null) =>
    {
      const token = await getToken();

      if (!token)
      {
        throw new Error(
          "Unable to obtain an authentication token for the current user profile.",
        );
      }

      return changeCurrentUserLastName(
      {
        token,
        lastName,
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