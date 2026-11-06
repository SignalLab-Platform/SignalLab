
"use client";

import { useState } from "react";
import { Check, ChevronsUpDown } from "lucide-react";

import { Button } from "@/components/ui/button";
import
{
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import
{
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";

import { countryOptions, getCountryName } from "./countries";

type CountryComboboxProps = Readonly<
{
  id?: string;
  value: string;
  onValueChange: (value: string) => void;
  disabled?: boolean;
}>;

export function CountryCombobox(
  { id, value, onValueChange, disabled = false }: CountryComboboxProps
)
{
  const [open, setOpen] = useState(false);

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          type="button"
          id={id}
          variant="outline"
          role="combobox"
          aria-label="Country"
          aria-expanded={open}
          disabled={disabled}
          className="w-full justify-between font-normal"
        >
          <span>
            {getCountryName(value) ?? "Select a country"}
          </span>

          <ChevronsUpDown className="size-4 opacity-50" />
        </Button>
      </PopoverTrigger>

      <PopoverContent
        align="start"
        className="w-(--anchor-width) p-0"
      >
        <Command>
          <CommandInput placeholder="Search countries..." />

          <CommandList>
            <CommandEmpty>
              No country found.
            </CommandEmpty>

            <CommandGroup>
              {countryOptions.map((country) =>
              {
                return (
                  <CommandItem
                    key={country.code}
                    value={country.name}
                    onSelect={() =>
                    {
                      onValueChange(country.code);
                      setOpen(false);
                    }}
                  >
                    <Check
                      className={
                        value === country.code
                          ? "size-4 opacity-100"
                          : "size-4 opacity-0"
                      }
                    />

                    {country.name}
                  </CommandItem>
                );
              })}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  );
}
