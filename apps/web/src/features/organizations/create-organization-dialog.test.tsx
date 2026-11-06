
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import { CreateOrganizationDialog } from "./create-organization-dialog";
import { useCreateOrganization } from "./use-create-organization";

jest.mock("./use-create-organization", () =>
{
  return {
    useCreateOrganization: jest.fn(),
  };
});

jest.mock("./country-combobox", () =>
{
  return {
    CountryCombobox: (
      {
        id,
        value,
        onValueChange,
        disabled,
      }: {
        id?: string;
        value: string;
        onValueChange: (value: string) => void;
        disabled?: boolean;
      }
    ) =>
    {
      return (
        <select
          id={id}
          aria-label="Country"
          value={value}
          disabled={disabled}
          onChange={(event) =>
          {
            onValueChange(event.target.value);
          }}
        >
          <option value="">Select a country</option>
          <option value="FR">France</option>
          <option value="DE">Germany</option>
        </select>
      );
    },
  };
});

const mockUseCreateOrganization = jest.mocked(useCreateOrganization);

const mockMutateAsync = jest.fn();
const mockReset = jest.fn();

function setupMutation(
  overrides: Record<string, unknown> = {}
)
{
  mockUseCreateOrganization.mockReturnValue(
  {
    mutateAsync: mockMutateAsync,
    reset: mockReset,
    isPending: false,
    isError: false,
    ...overrides,
  } as unknown as ReturnType<typeof useCreateOrganization>);
}

describe("CreateOrganizationDialog", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();
    setupMutation();
  });

  it("renders the form when opened", () =>
  {
    render(
      <CreateOrganizationDialog
        open={true}
        onOpenChange={jest.fn()}
      />
    );

    expect(
      screen.getByRole("dialog")
    ).toBeInTheDocument();

    expect(
      screen.getByRole("heading", { name: "Create Organization" })
    ).toBeInTheDocument();

    expect(
      screen.getByLabelText("Organization name")
    ).toBeInTheDocument();

    expect(
      screen.getByLabelText("Country")
    ).toBeInTheDocument();

    expect(
      screen.getByLabelText("Website")
    ).toBeInTheDocument();
  });

  it("disables submission until required fields are filled", async () =>
  {
    const user = userEvent.setup();

    render(
      <CreateOrganizationDialog
        open={true}
        onOpenChange={jest.fn()}
      />
    );

    const submit = screen.getByRole("button", {
      name: "Create Organization",
    });

    expect(submit).toBeDisabled();

    await user.type(
      screen.getByLabelText("Organization name"),
      "SignalLab Studio"
    );

    expect(submit).toBeDisabled();

    await user.selectOptions(
      screen.getByLabelText("Country"),
      "FR"
    );

    expect(submit).toBeEnabled();
  });

  it("submits the expected data and closes after success", async () =>
  {
    const user = userEvent.setup();
    const onOpenChange = jest.fn();

    mockMutateAsync.mockResolvedValue(
    {
      id: "organization-1",
      name: "SignalLab Studio",
      country: "FR",
      website: "https://example.com",
    });

    render(
      <CreateOrganizationDialog
        open={true}
        onOpenChange={onOpenChange}
      />
    );

    await user.type(
      screen.getByLabelText("Organization name"),
      "  SignalLab Studio  "
    );

    await user.selectOptions(
      screen.getByLabelText("Country"),
      "FR"
    );

    await user.type(
      screen.getByLabelText("Website"),
      "https://example.com"
    );

    await user.click(
      screen.getByRole("button", { name: "Create Organization" })
    );

    await waitFor(() =>
    {
      expect(mockMutateAsync).toHaveBeenCalledWith(
      {
        name: "SignalLab Studio",
        country: "FR",
        website: "https://example.com",
      });

      expect(onOpenChange).toHaveBeenCalledWith(false);
    });

    expect(mockReset).toHaveBeenCalled();
  });

  it("preserves form values when creation fails", async () =>
  {
    const user = userEvent.setup();
    const onOpenChange = jest.fn();

    mockMutateAsync.mockRejectedValue(new Error("API error"));

    render(
      <CreateOrganizationDialog
        open={true}
        onOpenChange={onOpenChange}
      />
    );

    await user.type(
      screen.getByLabelText("Organization name"),
      "SignalLab Studio"
    );

    await user.selectOptions(
      screen.getByLabelText("Country"),
      "FR"
    );

    await user.click(
      screen.getByRole("button", { name: "Create Organization" })
    );

    await waitFor(() =>
    {
      expect(mockMutateAsync).toHaveBeenCalled();
    });

    expect(
      screen.getByLabelText("Organization name")
    ).toHaveValue("SignalLab Studio");

    expect(
      screen.getByLabelText("Country")
    ).toHaveValue("FR");

    expect(onOpenChange).not.toHaveBeenCalledWith(false);
  });

  it("clears the form when cancelled", async () =>
  {
    const user = userEvent.setup();
    const onOpenChange = jest.fn();

    render(
      <CreateOrganizationDialog
        open={true}
        onOpenChange={onOpenChange}
      />
    );

    await user.type(
      screen.getByLabelText("Organization name"),
      "Temporary Organization"
    );

    await user.click(
      screen.getByRole("button", { name: "Cancel" })
    );

    expect(onOpenChange).toHaveBeenCalledWith(false);
    expect(mockReset).toHaveBeenCalled();

    expect(
      screen.getByLabelText("Organization name")
    ).toHaveValue("");
  });
});
