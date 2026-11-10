"use client";

import { useAuth } from "@clerk/nextjs";
import { useMutation, useQueryClient } from "@tanstack/react-query";

import {
  createOrganizationInvitation,
} from "@/features/organization-invitations/create-organization-invitation";

import type {
  CreateOrganizationInvitationInput,
} from "@/features/organization-invitations/organization-invitation";

import {
  organizationInvitationsQueryKey,
} from "@/features/organization-invitations/use-organization-invitations";

export function useCreateOrganizationInvitation(organizationId: string)
{
  const { getToken } = useAuth();
  const queryClient = useQueryClient();

  return useMutation(
  {
    mutationFn: async (input: CreateOrganizationInvitationInput) =>
    {
      const token = await getToken();

      if (!token)
      {
        throw new Error(
          "Unable to obtain an authentication token for the current user."
        );
      }

      return createOrganizationInvitation(
      {
        organizationId,
        input,
        token,
      });
    },

    onSuccess: async () =>
    {
      await queryClient.invalidateQueries(
      {
        queryKey: organizationInvitationsQueryKey(organizationId),
      });
    },
  });
}