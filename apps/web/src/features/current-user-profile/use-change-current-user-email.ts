"use client";

import { useAuth, useReverification, useUser } from "@clerk/nextjs";
import type { EmailAddressResource } from "@clerk/nextjs/types";
import { useQueryClient } from "@tanstack/react-query";
import { useState } from "react";

import
{
  promoteClerkEmailToPrimary,
  requestClerkEmailChange,
  verifyClerkEmailChange,
} from "@/features/current-user-profile/change-clerk-email";
import
{
  synchronizeCurrentUserEmail,
} from "@/features/current-user-profile/synchronize-current-user-email";
import
{
  currentUserProfileQueryKey,
} from "@/features/current-user-profile/use-current-user-profile";

type EmailChangeStep =
  | "idle"
  | "verification"
  | "promotion_failed"
  | "synchronization_failed"
  | "completed";

export function useChangeCurrentUserEmail()
{
  const { user } = useUser();
  const { getToken } = useAuth();
  const queryClient = useQueryClient();

  const [step, setStep] = useState<EmailChangeStep>("idle");
  const [pendingEmailAddress, setPendingEmailAddress] =
    useState<EmailAddressResource | null>(null);
  const [isPending, setIsPending] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const requestWithReverification = useReverification(
    async (email: string) =>
    {
      if (!user)
      {
        throw new Error("Clerk user is not available.");
      }

      return requestClerkEmailChange(user, email);
    },
  );

  const verifyWithReverification = useReverification(
    async (emailAddress: EmailAddressResource, code: string) =>
    {
      return verifyClerkEmailChange(emailAddress, code);
    },
  );

  const promoteWithReverification = useReverification(
    async (emailAddress: EmailAddressResource) =>
    {
      if (!user)
      {
        throw new Error("Clerk user is not available.");
      }

      await promoteClerkEmailToPrimary(user, emailAddress);
    },
  );

  async function synchronizeEmail()
  {
    const token = await getToken();

    if (!token)
    {
      throw new Error(
        "Unable to obtain an authentication token for email synchronization.",
      );
    }

    await synchronizeCurrentUserEmail(
    {
      token,
    });

    await queryClient.invalidateQueries(
    {
      queryKey: currentUserProfileQueryKey,
    });
  }

  async function promoteAndSynchronize(emailAddress: EmailAddressResource)
  {
    try
    {
      await promoteWithReverification(emailAddress);
    }
    catch (caughtError)
    {
      setStep("promotion_failed");
      setError(getErrorMessage(caughtError));
      return;
    }

    setStep("synchronization_failed");

    try
    {
      await synchronizeEmail();

      setStep("completed");
      setPendingEmailAddress(null);
    }
    catch (caughtError)
    {
      setError(getErrorMessage(caughtError));
    }
  }

  async function requestEmailChange(email: string)
  {
    if (isPending || step !== "idle")
    {
      return;
    }

    setIsPending(true);
    setError(null);

    try
    {
      const result = await requestWithReverification(email);

      setPendingEmailAddress(result.emailAddress);

      if (result.requiresVerification)
      {
        setStep("verification");
        return;
      }

      await promoteAndSynchronize(result.emailAddress);
    }
    catch (caughtError)
    {
      setError(getErrorMessage(caughtError));
    }
    finally
    {
      setIsPending(false);
    }
  }

  async function verifyEmailChange(code: string)
  {
    if (isPending || step !== "verification" || !pendingEmailAddress)
    {
      return;
    }

    setIsPending(true);
    setError(null);

    try
    {
      const verifiedEmailAddress = await verifyWithReverification(
        pendingEmailAddress,
        code,
      );

      setPendingEmailAddress(verifiedEmailAddress);

      await promoteAndSynchronize(verifiedEmailAddress);
    }
    catch (caughtError)
    {
      setError(getErrorMessage(caughtError));
    }
    finally
    {
      setIsPending(false);
    }
  }

  async function retryPromotion()
  {
    if (isPending || step !== "promotion_failed" || !pendingEmailAddress)
    {
      return;
    }

    setIsPending(true);
    setError(null);

    try
    {
      await promoteAndSynchronize(pendingEmailAddress);
    }
    finally
    {
      setIsPending(false);
    }
  }

  async function retrySynchronization()
  {
    if (isPending || step !== "synchronization_failed")
    {
      return;
    }

    setIsPending(true);
    setError(null);

    try
    {
      await synchronizeEmail();

      setStep("completed");
      setPendingEmailAddress(null);
    }
    catch (caughtError)
    {
      setError(getErrorMessage(caughtError));
    }
    finally
    {
      setIsPending(false);
    }
  }

  function reset()
  {
    if (
      isPending ||
      step === "promotion_failed" ||
      step === "synchronization_failed"
    )
    {
      return;
    }

    setStep("idle");
    setPendingEmailAddress(null);
    setError(null);
  }

  return {
    step,
    isPending,
    error,
    pendingEmail: pendingEmailAddress?.emailAddress ?? null,
    requestEmailChange,
    verifyEmailChange,
    retryPromotion,
    retrySynchronization,
    reset,
  };
}

function getErrorMessage(error: unknown): string
{
  if (error instanceof Error)
  {
    return error.message;
  }

  return "An unexpected error occurred while changing the email address.";
}