import type
{
  EmailAddressResource,
  UserResource,
} from "@clerk/nextjs/types";

export type ClerkEmailChangeRequest =
  | {
      requiresVerification: true;
      emailAddress: EmailAddressResource;
    }
  | {
      requiresVerification: false;
      emailAddress: EmailAddressResource;
    };

export async function requestClerkEmailChange(
  user: UserResource,
  email: string,
): Promise<ClerkEmailChangeRequest>
{
  const normalizedEmail = email.trim();

  const existingEmailAddress = user.emailAddresses.find(
    (address) =>
      address.emailAddress.toLowerCase() === normalizedEmail.toLowerCase(),
  );

  if (existingEmailAddress)
  {
    if (existingEmailAddress.verification.status === "verified")
    {
      return {
        requiresVerification: false,
        emailAddress: existingEmailAddress,
      };
    }

    const preparedEmailAddress = await existingEmailAddress.prepareVerification(
    {
      strategy: "email_code",
    });

    return {
      requiresVerification: true,
      emailAddress: preparedEmailAddress,
    };
  }

  const emailAddress = await user.createEmailAddress(
  {
    email: normalizedEmail,
  });

  const preparedEmailAddress = await emailAddress.prepareVerification(
  {
    strategy: "email_code",
  });

  return {
    requiresVerification: true,
    emailAddress: preparedEmailAddress,
  };
}

export async function verifyClerkEmailChange(
  emailAddress: EmailAddressResource,
  code: string,
): Promise<EmailAddressResource>
{
  const verifiedEmailAddress = await emailAddress.attemptVerification(
  {
    code: code.trim(),
  });

  if (verifiedEmailAddress.verification.status !== "verified")
  {
    throw new Error("Email address verification was not completed.");
  }

  return verifiedEmailAddress;
}

export async function promoteClerkEmailToPrimary(
  user: UserResource,
  emailAddress: EmailAddressResource,
): Promise<void>
{
  if (emailAddress.verification.status !== "verified")
  {
    throw new Error("Cannot promote an unverified email address.");
  }

  await user.update(
  {
    primaryEmailAddressId: emailAddress.id,
  });
}