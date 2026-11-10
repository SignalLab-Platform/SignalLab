export type OrganizationInvitationStatus =
  | "Pending"
  | "Accepted"
  | "Declined"
  | "Cancelled"
  | "Expired";

export type OrganizationInvitation = Readonly<
{
  id: string;
  organizationId: string;
  invitedUserId: string;
  invitedUserFirstName: string | null;
  invitedUserLastName: string | null;
  invitedUserEmail: string;
  invitedByUserId: string;
  roleId: string;
  roleName: string;
  status: OrganizationInvitationStatus;
  createdAt: string;
  updatedAt: string;
  expiresAt: string;
  respondedAt: string | null;
}>;

export type CurrentUserInvitation = Readonly<
{
  id: string;
  organizationId: string;
  organizationName: string;
  invitedByUserId: string;
  roleId: string;
  roleName: string;
  status: OrganizationInvitationStatus;
  createdAt: string;
  updatedAt: string;
  expiresAt: string;
  respondedAt: string | null;
}>;

export type OrganizationInvitationAction = Readonly<
{
  id: string;
  organizationId: string;
  invitedUserId: string;
  invitedByUserId: string;
  roleId: string;
  status: OrganizationInvitationStatus;
  createdAt: string;
  updatedAt: string;
  expiresAt: string;
  respondedAt: string | null;
}>;

export type CreateOrganizationInvitationInput = Readonly<
{
  email: string;
  roleId: string;
  duration: 1 | 7 | 30;
}>;