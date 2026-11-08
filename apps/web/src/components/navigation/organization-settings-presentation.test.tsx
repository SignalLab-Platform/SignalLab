import { fireEvent, render, screen, within } from "@testing-library/react";

import
{
  OrganizationSettingsContent,
} from "@/components/navigation/organization-settings-presentation";

import type
{
  Organization,
} from "@/features/organizations/organization";

const organization: Organization =
{
  id: "organization-alpha",
  name: "Studio Alpha",
  country: "FR",
  website: "https://studio-alpha.example",
};

function renderSettings(
  overrides: Partial<React.ComponentProps<typeof OrganizationSettingsContent>> = {}
)
{
  const onRetry = jest.fn();

  render(
    <OrganizationSettingsContent
      organization={organization}
      isPending={false}
      isError={false}
      onRetry={onRetry}
      roles={[]}
      rolesIsPending={false}
      rolesIsError={false}
      onRetryRoles={jest.fn()}
      {...overrides}
    />
  );

  return { onRetry };
}

describe("OrganizationSettingsContent", () =>
{
  it("displays real organization information", () =>
  {
    renderSettings();

    expect(screen.getByText("Studio Alpha")).toBeInTheDocument();
    expect(screen.getByText("FR")).toBeInTheDocument();

    expect(
      screen.getByText("https://studio-alpha.example")
    ).toBeInTheDocument();
  });

  it("distinguishes unavailable fields from missing values", () =>
  {
    renderSettings(
    {
      organization:
      {
        ...organization,
        website: null,
      },
    });

    expect(screen.getAllByText("Not available yet")).toHaveLength(2);
    expect(screen.getByText("Not provided")).toBeInTheDocument();
  });

  it("provides a separate disabled Edit action for each field", () =>
  {
    renderSettings();

    for (const field of ["name", "country", "description", "website"])
    {
      expect(
        screen.getByRole("button",
        {
          name: `Edit ${field}`,
        })
      ).toBeDisabled();
    }

    expect(
      screen.queryByRole("button",
      {
        name: "Edit settings",
      })
    ).not.toBeInTheDocument();
  });

  it("keeps logo management disabled", () =>
  {
    renderSettings();

    expect(
      screen.getByRole("button",
      {
        name: "Change",
      })
    ).toBeDisabled();

    expect(
      screen.getByText("Organization logo")
    ).toBeInTheDocument();
  });

  it("displays a disabled Delete organization action", () =>
  {
    renderSettings();

    const dangerZone = screen.getByRole("region",
    {
      name: "Danger zone",
    });

    expect(
      within(dangerZone).getByRole("button",
      {
        name: "Delete organization",
      })
    ).toBeDisabled();
  });

  it("displays a loading state", () =>
  {
    renderSettings(
    {
      organization: undefined,
      isPending: true,
    });

    expect(screen.getByRole("status")).toHaveTextContent(
      "Loading organization settings..."
    );
  });

  it("displays an error and supports retry", () =>
  {
    const { onRetry } = renderSettings(
    {
      organization: undefined,
      isError: true,
    });

    expect(screen.getByRole("alert")).toHaveTextContent(
      "Unable to load organization settings."
    );

    fireEvent.click(
      screen.getByRole("button",
      {
        name: "Retry",
      })
    );

    expect(onRetry).toHaveBeenCalledTimes(1);
  });

  it("displays an unavailable state when the organization is missing", () =>
  {
    renderSettings(
    {
      organization: undefined,
    });

    expect(screen.getByRole("status")).toHaveTextContent(
      "Organization unavailable."
    );
  });
});