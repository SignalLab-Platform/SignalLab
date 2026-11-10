"use client";

import { useAuth } from "@clerk/nextjs";
import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  acceptOrganizationInvitation,
} from "@/features/organization-invitations/accept-organization-invitation";

import {
  organizationInvitationQueryKey,
} from "@/features/organization-invitations/use-organization-invitation";

import {
  currentUserInvitationsQueryKey,
} from "@/features/organization-invitations/use-current-user-invitations";

import {
  organizationsQueryKey,
} from "@/features/organizations/use-organizations";

export function useAcceptOrganizationInvitation(invitationId: string)
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

      return acceptOrganizationInvitation(
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

        queryClient.invalidateQueries(
        {
          queryKey: organizationsQueryKey,
        }),
      ]);
    },
  });
}