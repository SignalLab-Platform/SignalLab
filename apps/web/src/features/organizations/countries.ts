
import countries from "i18n-iso-countries";
import englishLocale from "i18n-iso-countries/langs/en.json";

countries.registerLocale(englishLocale);

export type CountryOption = Readonly<
{
  code: string;
  name: string;
}>;

export const countryOptions: readonly CountryOption[] = Object.entries(
  countries.getNames("en", { select: "official" })
)
  .map(([code, name]) =>
  {
    return { code, name };
  })
  .sort((a, b) =>
  {
    return a.name.localeCompare(b.name, "en");
  });

export function getCountryName(code: string): string | undefined
{
  return countryOptions.find((country) =>
  {
    return country.code === code;
  })?.name;
}
