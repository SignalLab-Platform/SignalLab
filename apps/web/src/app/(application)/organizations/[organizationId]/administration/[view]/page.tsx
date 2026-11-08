import
{
  NavigationRoutePage,
} from "@/components/navigation/navigation-route-page";

type OrganizationAdministrationPageProps =
{
  params: Promise<
  {
    organizationId: string;
    view: string;
  }>;
};

export default async function OrganizationAdministrationPage(
  { params }: Readonly<OrganizationAdministrationPageProps>
)
{
  const { organizationId, view } = await params;

  return (
    <NavigationRoutePage
      segments={[
        "organizations",
        organizationId,
        "administration",
        view,
      ]}
    />
  );
}