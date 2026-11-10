import {
  NavigationRoutePage,
} from "@/components/navigation/navigation-route-page";

type OrganizationInvitationPageProps = Readonly<
{
  params: Promise<
  {
    invitationId: string;
  }>;
}>;

export default async function OrganizationInvitationPage(
  { params }: OrganizationInvitationPageProps
)
{
  const { invitationId } = await params;

  return (
    <NavigationRoutePage
      segments={["organization-invitations", invitationId]}
    />
  );
}