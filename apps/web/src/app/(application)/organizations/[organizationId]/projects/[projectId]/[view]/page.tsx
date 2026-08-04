import
{
  NavigationRoutePage,
} from "@/components/navigation/navigation-route-page";

type ProjectPageProps =
{
  params: Promise<
  {
    organizationId: string;
    projectId: string;
    view: string;
  }>;
};

export default async function ProjectPage(
  { params }: Readonly<ProjectPageProps>
)
{
  const { organizationId, projectId, view } = await params;

  return (
    <NavigationRoutePage
      segments={[
        "organizations",
        organizationId,
        "projects",
        projectId,
        view,
      ]}
    />
  );
}
