"use client";

import { useAuth, useUser } from "@clerk/nextjs";
import { useMutation, useQueryClient } from "@tanstack/react-query";

import
{
  synchronizeCurrentUserEmail,
} from "@/features/current-user-profile/synchronize-current-user-email";
import
{
  currentUserProfileQueryKey,
} from "@/features/current-user-profile/use-current-user-profile";

export function useCurrentUserEmailReconciliation()
{
  const { getToken } = useAuth();
  const { user } = useUser();
  const queryClient = useQueryClient();

  const mutation = useMutation(
  {
    mutationFn: async () =>
    {
      const token = await getToken();

      if (!token)
      {
        throw new Error(
          "Unable to obtain an authentication token for email synchronization.",
        );
      }

      return synchronizeCurrentUserEmail(
      {
        token,
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

  function needsReconciliation(profileEmail: string)
  {
    const clerkEmail = user?.primaryEmailAddress?.emailAddress;

    if (!clerkEmail)
    {
      return false;
    }

    return clerkEmail.trim().toLowerCase() !==
      profileEmail.trim().toLowerCase();
  }

  return {
    needsReconciliation,
    reconcile: mutation.mutateAsync,
    isPending: mutation.isPending,
    isError: mutation.isError,
    error: mutation.error,
    reset: mutation.reset,
  };
}