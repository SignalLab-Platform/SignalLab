"use client";

import { useAuth } from "@clerk/nextjs";
import { useMutation, useQueryClient } from "@tanstack/react-query";

import
{
  createOrganization,
  type CreateOrganizationInput,
} from "@/features/organizations/create-organization";

import { organizationsQueryKey } from "@/features/organizations/use-organizations";

export function useCreateOrganization()
{
  const { getToken } = useAuth();
  const queryClient = useQueryClient();

  return useMutation(
  {
    mutationFn: async (input: CreateOrganizationInput) =>
    {
      const token = await getToken();

      if (!token)
      {
        throw new Error(
          "Unable to obtain an authentication token for the current user."
        );
      }

      return createOrganization(
      {
        token,
        input,
      });
    },

    onSuccess: async () =>
    {
      await queryClient.invalidateQueries(
      {
        queryKey: organizationsQueryKey,
      });
    },
  });
}