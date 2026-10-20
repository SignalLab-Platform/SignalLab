
"use client";

import { useState } from "react";

import { Button } from "@/components/ui/button";
import
{
  useChangeCurrentUserEmail,
} from "@/features/current-user-profile/use-change-current-user-email";
import
{
  useCurrentUserEmailReconciliation,
} from "@/features/current-user-profile/use-current-user-email-reconciliation";

type ProfileEmailEditorProps = Readonly<
{
  email: string;
}>;

export function ProfileEmailEditor({ email }: ProfileEmailEditorProps)
{
  const changeEmail = useChangeCurrentUserEmail();
  const reconciliation = useCurrentUserEmailReconciliation();

  const [isEditing, setIsEditing] = useState(false);
  const [newEmail, setNewEmail] = useState(email);
  const [verificationCode, setVerificationCode] = useState("");

  const needsReconciliation = reconciliation.needsReconciliation(email);

  const isBusy = changeEmail.isPending || reconciliation.isPending;

  function handleEdit()
  {
    setNewEmail(email);
    setVerificationCode("");
    setIsEditing(true);
  }

  function handleCancel()
  {
    changeEmail.reset();
    setIsEditing(false);
    setVerificationCode("");
  }

  async function handleRequest()
  {
    const normalizedEmail = newEmail.trim();

    if (
      !normalizedEmail ||
      normalizedEmail.toLowerCase() === email.trim().toLowerCase()
    )
    {
      return;
    }

    await changeEmail.requestEmailChange(normalizedEmail);
  }

  async function handleVerify()
  {
    if (!verificationCode.trim())
    {
      return;
    }

    await changeEmail.verifyEmailChange(verificationCode);
  }

  function handleComplete()
  {
    changeEmail.reset();
    setIsEditing(false);
    setVerificationCode("");
  }

  if (changeEmail.step === "promotion_failed")
  {
    return (
      <div className="space-y-3 py-5">
        <EmailField email={email} />

        <p className="text-sm text-destructive">
          Your new email was verified, but it could not be set as
          your primary email address.
        </p>

        <ErrorMessage message={changeEmail.error} />

        <Button
          onClick={() => void changeEmail.retryPromotion()}
          disabled={isBusy}
        >
          {isBusy ? "Retrying..." : "Retry email update"}
        </Button>
      </div>
    );
  }

  if (changeEmail.step === "synchronization_failed")
  {
    return (
      <div className="space-y-3 py-5">
        <EmailField email={email} />

        <p className="text-sm text-destructive">
          Your primary email was updated in Clerk, but SignalLab
          could not synchronize your profile.
        </p>

        <ErrorMessage message={changeEmail.error} />

        <Button
          onClick={() => void changeEmail.retrySynchronization()}
          disabled={isBusy}
        >
          {isBusy ? "Synchronizing..." : "Retry synchronization"}
        </Button>
      </div>
    );
  }

  if (changeEmail.step === "completed")
  {
    return (
      <div className="space-y-3 py-5">
        <EmailField email={email} />

        <p className="text-sm text-muted-foreground">
          Your email address has been updated successfully.
        </p>

        <Button
          variant="outline"
          onClick={handleComplete}
        >
          Done
        </Button>
      </div>
    );
  }

  if (needsReconciliation)
  {
    return (
      <div className="space-y-3 py-5">
        <EmailField email={email} />

        <p className="text-sm text-destructive">
          Your SignalLab email address is out of sync with your
          primary authentication email.
        </p>

        {reconciliation.isError &&
          <ErrorMessage
            message={
              reconciliation.error instanceof Error
                ? reconciliation.error.message
                : "Unable to synchronize your email address."
            }
          />
        }

        <Button
          onClick={() => void reconciliation.reconcile()}
          disabled={isBusy}
        >
          {isBusy ? "Synchronizing..." : "Retry synchronization"}
        </Button>
      </div>
    );
  }

  if (changeEmail.step === "verification")
  {
    return (
      <div className="space-y-4 py-5">
        <EmailField email={email} />

        <p className="text-sm text-muted-foreground">
          Enter the verification code sent to{" "}
          <span className="font-medium text-foreground">
            {changeEmail.pendingEmail}
          </span>.
        </p>

        <EmailInput
          id="email-verification-code"
          label="Verification code"
          type="text"
          value={verificationCode}
          onChange={setVerificationCode}
          disabled={isBusy}
          autoComplete="one-time-code"
        />

        <ErrorMessage message={changeEmail.error} />

        <div className="flex gap-2">
          <Button
            onClick={() => void handleVerify()}
            disabled={isBusy || !verificationCode.trim()}
          >
            {isBusy ? "Verifying..." : "Verify"}
          </Button>

          <Button
            variant="outline"
            onClick={handleCancel}
            disabled={isBusy}
          >
            Cancel
          </Button>
        </div>
      </div>
    );
  }

  if (isEditing)
  {
    return (
      <div className="space-y-4 py-5">
        <EmailInput
          id="profile-email"
          label="Email"
          type="email"
          value={newEmail}
          onChange={setNewEmail}
          disabled={isBusy}
          autoComplete="email"
        />

        <ErrorMessage message={changeEmail.error} />

        <div className="flex gap-2">
          <Button
            onClick={() => void handleRequest()}
            disabled={
              isBusy ||
              !newEmail.trim() ||
              newEmail.trim().toLowerCase() === email.trim().toLowerCase()
            }
          >
            {isBusy ? "Saving..." : "Save"}
          </Button>

          <Button
            variant="outline"
            onClick={handleCancel}
            disabled={isBusy}
          >
            Cancel
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="flex items-start justify-between gap-6 py-5">
      <EmailField email={email} />

      <Button
        variant="outline"
        onClick={handleEdit}
      >
        Edit
      </Button>
    </div>
  );
}

function EmailField({ email }: Readonly<{ email: string }>)
{
  return (
    <dl className="min-w-0 flex-1 space-y-1">
      <dt className="text-sm font-medium">
        Email
      </dt>

      <dd className="text-sm text-muted-foreground">
        {email}
      </dd>
    </dl>
  );
}

type EmailInputProps = Readonly<
{
  id: string;
  label: string;
  type: string;
  value: string;
  disabled: boolean;
  autoComplete: string;
  onChange: (value: string) => void;
}>;

function EmailInput(
{
  id,
  label,
  type,
  value,
  disabled,
  autoComplete,
  onChange,
}: EmailInputProps)
{
  return (
    <div className="space-y-2">
      <label
        htmlFor={id}
        className="text-sm font-medium"
      >
        {label}
      </label>

      <input
        id={id}
        type={type}
        value={value}
        disabled={disabled}
        autoComplete={autoComplete}
        onChange={(event) => onChange(event.target.value)}
        className="h-9 w-full rounded-md border border-input bg-background px-3 text-sm outline-none transition-colors focus-visible:border-ring focus-visible:ring-2 focus-visible:ring-ring/50 disabled:cursor-not-allowed disabled:opacity-50"
      />
    </div>
  );
}

function ErrorMessage({ message }: Readonly<{ message: string | null }>)
{
  if (!message)
  {
    return null;
  }

  return (
    <p role="alert" className="text-sm text-destructive">
      {message}
    </p>
  );
}
