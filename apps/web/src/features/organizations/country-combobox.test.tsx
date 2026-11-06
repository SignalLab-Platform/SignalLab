
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import { CountryCombobox } from "./country-combobox";

class MockResizeObserver
{
  observe()
  {
  }

  unobserve()
  {
  }

  disconnect()
  {
  }
}

global.ResizeObserver = MockResizeObserver;

Element.prototype.scrollIntoView = jest.fn();

describe("CountryCombobox", () =>
{
  it("displays the placeholder when no country is selected", () =>
  {
    render(
      <CountryCombobox
        value=""
        onValueChange={jest.fn()}
      />
    );

    expect(
      screen.getByRole("combobox", { name: "Country" })
    ).toHaveTextContent("Select a country");
  });

  it("displays the selected country name", () =>
  {
    render(
      <CountryCombobox
        value="FR"
        onValueChange={jest.fn()}
      />
    );

    expect(
      screen.getByRole("combobox", { name: "Country" })
    ).toHaveTextContent("France");
  });

  it("selects a country and returns its ISO code", async () =>
  {
    const user = userEvent.setup();
    const onValueChange = jest.fn();

    render(
      <CountryCombobox
        value=""
        onValueChange={onValueChange}
      />
    );

    await user.click(screen.getByRole("combobox"));

    const search = screen.getByPlaceholderText("Search countries...");
    await user.type(search, "France");

    await user.click(
      await screen.findByRole("option", { name: /france/i })
    );

    expect(onValueChange).toHaveBeenCalledWith("FR");

    await waitFor(() =>
    {
      expect(screen.queryByPlaceholderText("Search countries..."))
        .not.toBeInTheDocument();
    });
  });

  it("does not open when disabled", async () =>
  {
    const user = userEvent.setup();

    render(
      <CountryCombobox
        value=""
        onValueChange={jest.fn()}
        disabled
      />
    );

    const trigger = screen.getByRole("combobox");

    expect(trigger).toBeDisabled();

    await user.click(trigger);

    expect(
      screen.queryByPlaceholderText("Search countries...")
    ).not.toBeInTheDocument();
  });
});
