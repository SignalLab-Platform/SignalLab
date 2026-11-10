export const ORGANIZATION_INVITATION_ROUTE_PREFIX =
  "/organization-invitations/";

export function buildOrganizationInvitationRoute(
  invitationId: string,
  originPath?: string | null
): string
{
  const pathname =
    `${ORGANIZATION_INVITATION_ROUTE_PREFIX}${encodeURIComponent(invitationId)}`;

  if (!originPath)
  {
    return pathname;
  }

  const safeOrigin = getSafeInvitationOrigin(originPath);

  if (safeOrigin === null)
  {
    return pathname;
  }

  const searchParams = new URLSearchParams(
  {
    from: safeOrigin,
  });

  return `${pathname}?${searchParams.toString()}`;
}

export function getSafeInvitationOrigin(
  value: string | null | undefined
): string | null
{
  if (
    !value ||
    !value.startsWith("/") ||
    value.startsWith("//") ||
    value.includes("\\")
  )
  {
    return null;
  }

  try
  {
    const url = new URL(value, "https://signallab.invalid");

    if (
      url.origin !== "https://signallab.invalid" ||
      !url.pathname.startsWith("/") ||
      url.pathname.startsWith("//") ||
      url.pathname.startsWith(ORGANIZATION_INVITATION_ROUTE_PREFIX)
    )
    {
      return null;
    }

    if (
      url.username.length > 0 ||
      url.password.length > 0
    )
    {
      return null;
    }

    return `${url.pathname}${url.search}${url.hash}`;
  }
  catch
  {
    return null;
  }
}