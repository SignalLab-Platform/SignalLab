import
{
  NavigationRoutePage,
} from "@/components/navigation/navigation-route-page";

type CampaignPageProps =
{
  params: Promise<
  {
    organizationId: string;
    projectId: string;
    campaignId: string;
    view: string;
  }>;
};

export default async function CampaignPage(
  { params }: Readonly<CampaignPageProps>
)
{
  const {
    organizationId,
    projectId,
    campaignId,
    view,
  } = await params;

  return (
    <NavigationRoutePage
      segments={[
        "organizations",
        organizationId,
        "projects",
        projectId,
        "campaigns",
        campaignId,
        view,
      ]}
    />
  );
}
