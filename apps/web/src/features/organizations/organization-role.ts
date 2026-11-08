export type OrganizationRole = Readonly<
{
  id: string;
  name: string;
  description: string | null;
  isOwner: boolean;
  permissions: string[];
  createdAt: string;
  updatedAt: string;
}>;