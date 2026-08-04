import { render, screen } from "@testing-library/react";

import { LocalNavigation } from "@/components/navigation/local-navigation";
import { resolveNavigationLocation } from "@/navigation/navigation-resolver";

function resolveRequiredLocation(pathname: string)
{
  const location = resolveNavigationLocation(pathname);

  if (location === null)
  {
    throw new Error(`Unable to resolve test pathname: ${pathname}`);
  }

  return location;
}

describe("LocalNavigation", () =>
{
  it("does not render Local Navigation for Personal Home", () =>
  {
    const location = resolveRequiredLocation("/home");

    const { container } = render(
      <LocalNavigation location={location} />
    );

    expect(container).toBeEmptyDOMElement();
  });

  it("changes Campaign views without changing its context hierarchy", () =>
  {
    const location = resolveRequiredLocation(
      "/organizations/demo-organization/projects/demo-project/campaigns/demo-campaign/results"
    );

    render(<LocalNavigation location={location} />);

    expect(
      screen.getByRole("link",
      {
        name: "Overview",
      })
    ).toHaveAttribute(
      "href",
      "/organizations/demo-organization/projects/demo-project/campaigns/demo-campaign/overview"
    );

    expect(
      screen.getByRole("link",
      {
        name: "Results",
      })
    ).toHaveAttribute("aria-current", "page");

    expect(
      screen.getByRole("link",
      {
        name: "Settings",
      })
    ).toHaveAttribute(
      "href",
      "/organizations/demo-organization/projects/demo-project/campaigns/demo-campaign/settings"
    );
  });

  it("does not render tabs for a Participation Document", () =>
  {
    const location = resolveRequiredLocation(
      "/participations/demo-participation/consent"
    );

    const { container } = render(
      <LocalNavigation location={location} />
    );

    expect(container).toBeEmptyDOMElement();
  });
});
