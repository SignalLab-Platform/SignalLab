
import type
{
  EmailAddressResource,
  UserResource,
} from "@clerk/nextjs/types";

import
{
  promoteClerkEmailToPrimary,
  requestClerkEmailChange,
  verifyClerkEmailChange,
} from "@/features/current-user-profile/change-clerk-email";

describe("changeClerkEmail", () =>
{
  const emailAddressId = "idn_new_email";

  function createEmailAddress(
    status: "verified" | "unverified" = "unverified",
    email = "new@example.com",
  )
  {
    const emailAddress =
    {
      id: emailAddressId,
      emailAddress: email,
      verification:
      {
        status,
      },
      prepareVerification: jest.fn(),
      attemptVerification: jest.fn(),
    };

    emailAddress.prepareVerification.mockResolvedValue(emailAddress);
    emailAddress.attemptVerification.mockResolvedValue(emailAddress);

    return emailAddress as typeof emailAddress & EmailAddressResource;
  }

  function createUser(
    emailAddress: EmailAddressResource,
    existingEmailAddresses: EmailAddressResource[] = [],
  )
  {
    return {
      emailAddresses: existingEmailAddresses,
      createEmailAddress: jest.fn().mockResolvedValue(emailAddress),
      update: jest.fn().mockResolvedValue({}),
    } as unknown as UserResource;
  }

  it("creates an email address and requests an OTP", async () =>
  {
    const emailAddress = createEmailAddress();
    const user = createUser(emailAddress);

    const result = await requestClerkEmailChange(
      user,
      "  new@example.com  ",
    );

    expect(user.createEmailAddress).toHaveBeenCalledWith(
    {
      email: "new@example.com",
    });

    expect(emailAddress.prepareVerification).toHaveBeenCalledWith(
    {
      strategy: "email_code",
    });

    expect(result).toEqual(
    {
      requiresVerification: true,
      emailAddress,
    });
  });

  it("reuses an existing verified email without requesting an OTP", async () =>
  {
    const emailAddress = createEmailAddress(
      "verified",
      "old@example.com",
    );

    const user = createUser(emailAddress, [emailAddress]);

    const result = await requestClerkEmailChange(
      user,
      "  OLD@example.com  ",
    );

    expect(result).toEqual(
    {
      requiresVerification: false,
      emailAddress,
    });

    expect(user.createEmailAddress).not.toHaveBeenCalled();
    expect(emailAddress.prepareVerification).not.toHaveBeenCalled();
  });

  it("requests an OTP for an existing unverified email", async () =>
  {
    const emailAddress = createEmailAddress(
      "unverified",
      "old@example.com",
    );

    const user = createUser(emailAddress, [emailAddress]);

    const result = await requestClerkEmailChange(
      user,
      "old@example.com",
    );

    expect(result).toEqual(
    {
      requiresVerification: true,
      emailAddress,
    });

    expect(user.createEmailAddress).not.toHaveBeenCalled();

    expect(emailAddress.prepareVerification).toHaveBeenCalledWith(
    {
      strategy: "email_code",
    });
  });

  it("creates a new address when other emails already exist", async () =>
  {
    const newEmailAddress = createEmailAddress();
    const existingEmailAddress = createEmailAddress(
      "verified",
      "old@example.com",
    );

    const user = createUser(
      newEmailAddress,
      [existingEmailAddress],
    );

    const result = await requestClerkEmailChange(
      user,
      "new@example.com",
    );

    expect(user.createEmailAddress).toHaveBeenCalledWith(
    {
      email: "new@example.com",
    });

    expect(result.requiresVerification).toBe(true);
    expect(result.emailAddress).toBe(newEmailAddress);
  });

  it("verifies the OTP and returns the verified email address", async () =>
  {
    const emailAddress = createEmailAddress("verified");
    const user = createUser(emailAddress);

    const result = await verifyClerkEmailChange(
      emailAddress,
      " 123456 ",
    );

    expect(emailAddress.attemptVerification).toHaveBeenCalledWith(
    {
      code: "123456",
    });

    expect(result).toBe(emailAddress);
    expect(user.update).not.toHaveBeenCalled();
  });

  it("rejects an incomplete email verification", async () =>
  {
    const emailAddress = createEmailAddress("unverified");

    await expect(
      verifyClerkEmailChange(emailAddress, "000000"),
    ).rejects.toThrow(
      "Email address verification was not completed.",
    );
  });

  it("promotes a verified email address to primary", async () =>
  {
    const emailAddress = createEmailAddress("verified");
    const user = createUser(emailAddress);

    await promoteClerkEmailToPrimary(user, emailAddress);

    expect(user.update).toHaveBeenCalledWith(
    {
      primaryEmailAddressId: emailAddressId,
    });
  });

  it("rejects promotion of an unverified email address", async () =>
  {
    const emailAddress = createEmailAddress("unverified");
    const user = createUser(emailAddress);

    await expect(
      promoteClerkEmailToPrimary(user, emailAddress),
    ).rejects.toThrow(
      "Cannot promote an unverified email address.",
    );

    expect(user.update).not.toHaveBeenCalled();
  });

  it("propagates an error when Clerk cannot create the email", async () =>
  {
    const emailAddress = createEmailAddress();
    const user = createUser(emailAddress);

    jest.mocked(user.createEmailAddress).mockRejectedValue(
      new Error("Clerk creation failed."),
    );

    await expect(
      requestClerkEmailChange(user, "new@example.com"),
    ).rejects.toThrow("Clerk creation failed.");

    expect(emailAddress.prepareVerification).not.toHaveBeenCalled();
  });

  it("propagates an error when Clerk cannot verify an existing email", async () =>
  {
    const emailAddress = createEmailAddress(
      "unverified",
      "old@example.com",
    );

    const user = createUser(emailAddress, [emailAddress]);

    emailAddress.prepareVerification.mockRejectedValue(
      new Error("Clerk verification preparation failed."),
    );

    await expect(
      requestClerkEmailChange(user, "old@example.com"),
    ).rejects.toThrow("Clerk verification preparation failed.");

    expect(user.createEmailAddress).not.toHaveBeenCalled();
  });

  it("propagates an error when Clerk rejects the OTP", async () =>
  {
    const emailAddress = createEmailAddress();

    emailAddress.attemptVerification.mockRejectedValue(
      new Error("Invalid verification code."),
    );

    await expect(
      verifyClerkEmailChange(emailAddress, "000000"),
    ).rejects.toThrow("Invalid verification code.");
  });

  it("propagates an error when Clerk cannot promote the email", async () =>
  {
    const emailAddress = createEmailAddress("verified");
    const user = createUser(emailAddress);

    jest.mocked(user.update).mockRejectedValue(
      new Error("Clerk promotion failed."),
    );

    await expect(
      promoteClerkEmailToPrimary(user, emailAddress),
    ).rejects.toThrow("Clerk promotion failed.");
  });
});
