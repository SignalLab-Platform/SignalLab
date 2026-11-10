"use client";

import { useAuth } from "@clerk/nextjs";
import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  cancelOrganizationInvitation,
} from "@/features/organization-invitations/cancel-organization-invitation";

import {
  organizationInvitationsQueryKey,
} from "@/features/organization-invitations/use-organization-invitations";

import {
  organizationInvitationQueryKey,
} from "@/features/organization-invitations/use-organization-invitation";

export function useCancelOrganizationInvitation(organizationId: string)
{
  const { getToken } = useAuth();
  const queryClient = useQueryClient();

  return useMutation(
  {
    mutationFn: async (invitationId: string) =>
    {
      const token = await getToken();

      if (!token)
      {
        throw new Error(
          "Unable to obtain an authentication token for the current user."
        );
      }

      return cancelOrganizationInvitation(
      {
        invitationId,
        token,
      });
    },

    onSuccess: async (_, invitationId) =>
    {
      await Promise.all(
      [
        queryClient.invalidateQueries(
        {
          queryKey: organizationInvitationsQueryKey(organizationId),
        }),

        queryClient.invalidateQueries(
        {
          queryKey: organizationInvitationQueryKey(invitationId),
        }),
      ]);
    },
  });
}