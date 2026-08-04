import
{
  NavigationRoutePage,
} from "@/components/navigation/navigation-route-page";

type OrganizationPageProps =
{
  params: Promise<
  {
    organizationId: string;
    view: string;
  }>;
};

export default async function OrganizationPage(
  { params }: Readonly<OrganizationPageProps>
)
{
  const { organizationId, view } = await params;

  return (
    <NavigationRoutePage
      segments={[
        "organizations",
        organizationId,
        view,
      ]}
    />
  );
}
