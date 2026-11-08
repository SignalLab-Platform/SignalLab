export type OrganizationMember = Readonly<
{
  id: string;
  userId: string;
  firstName: string | null;
  lastName: string | null;
  email: string;
  roleId: string;
  roleName: string;
  status: string;
  joinedAt: string;
  removedAt: string | null;
}>;