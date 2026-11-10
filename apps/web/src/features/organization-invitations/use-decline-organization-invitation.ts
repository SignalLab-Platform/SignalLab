"use client";

import { useAuth } from "@clerk/nextjs";
import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  declineOrganizationInvitation,
} from "@/features/organization-invitations/decline-organization-invitation";

import {
  organizationInvitationQueryKey,
} from "@/features/organization-invitations/use-organization-invitation";

import {
  currentUserInvitationsQueryKey,
} from "@/features/organization-invitations/use-current-user-invitations";

export function useDeclineOrganizationInvitation(invitationId: string)
{
  const { getToken } = useAuth();
  const queryClient = useQueryClient();

  return useMutation(
  {
    mutationFn: async () =>
    {
      const token = await getToken();

      if (!token)
      {
        throw new Error(
          "Unable to obtain an authentication token for the current user."
        );
      }

      return declineOrganizationInvitation(
      {
        invitationId,
        token,
      });
    },

    onSuccess: async () =>
    {
      await Promise.all(
      [
        queryClient.invalidateQueries(
        {
          queryKey: organizationInvitationQueryKey(invitationId),
        }),

        queryClient.invalidateQueries(
        {
          queryKey: currentUserInvitationsQueryKey,
        }),
      ]);
    },
  });
}