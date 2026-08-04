import { notFound } from "next/navigation";

import
{
  NavigationPresentation,
} from "@/components/navigation/navigation-presentation";

import
{
  resolveNavigationLocation,
} from "@/navigation/navigation-resolver";

type NavigationRoutePageProps =
{
  segments: readonly string[];
};

function buildPathnameFromSegments(segments: readonly string[]): string
{
  if (segments.length === 0)
  {
    return "/";
  }

  const encodedSegments = segments.map((segment) =>
  {
    return encodeURIComponent(segment);
  });

  return `/${encodedSegments.join("/")}`;
}

export function NavigationRoutePage(
  { segments }: Readonly<NavigationRoutePageProps>
)
{
  const pathname = buildPathnameFromSegments(segments);
  const location = resolveNavigationLocation(pathname);

  if (location === null)
  {
    notFound();
  }

  return (
    <NavigationPresentation location={location} />
  );
}
