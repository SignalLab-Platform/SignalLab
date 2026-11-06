
import { countryOptions, getCountryName } from "./countries";

describe("countries", () =>
{
  it("provides ISO country codes and English names", () =>
  {
    expect(countryOptions).toContainEqual(
    {
      code: "FR",
      name: "France",
    });

    expect(countryOptions).toContainEqual(
    {
      code: "DE",
      name: "Germany",
    });
  });

  it("sorts countries alphabetically by name", () =>
  {
    const names = countryOptions.map((country) => country.name);

    expect(names).toEqual(
      [...names].sort((a, b) => a.localeCompare(b, "en"))
    );
  });

  it("resolves a country name from its ISO code", () =>
  {
    expect(getCountryName("FR")).toBe("France");
    expect(getCountryName("ZZ")).toBeUndefined();
  });
});
