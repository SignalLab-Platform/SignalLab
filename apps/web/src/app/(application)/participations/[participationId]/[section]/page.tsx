import
{
  NavigationRoutePage,
} from "@/components/navigation/navigation-route-page";

type ParticipationPageProps =
{
  params: Promise<
  {
    participationId: string;
    section: string;
  }>;
};

export default async function ParticipationPage(
  { params }: Readonly<ParticipationPageProps>
)
{
  const { participationId, section } = await params;

  return (
    <NavigationRoutePage
      segments={[
        "participations",
        participationId,
        section,
      ]}
    />
  );
}
