"use client";

import { useState, type FormEvent } from "react";

import { Info } from "lucide-react";

import { Button } from "@/components/ui/button";
import
{
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

import { CountryCombobox } from "./country-combobox";
import { useCreateOrganization } from "./use-create-organization";

type CreateOrganizationDialogProps = Readonly<
{
  open: boolean;
  onOpenChange: (open: boolean) => void;
}>;

export function CreateOrganizationDialog(
  { open, onOpenChange }: CreateOrganizationDialogProps
)
{
  const [name, setName] = useState("");
  const [country, setCountry] = useState("");
  const [website, setWebsite] = useState("");

  const createOrganization = useCreateOrganization();

  const canSubmit =
    name.trim().length > 0 &&
    country.length > 0 &&
    !createOrganization.isPending;

  function closeDialog()
  {
    setName("");
    setCountry("");
    setWebsite("");
    createOrganization.reset();
    onOpenChange(false);
  }

  function handleOpenChange(nextOpen: boolean)
  {
    if (createOrganization.isPending)
    {
      return;
    }

    if (!nextOpen)
    {
      closeDialog();
      return;
    }

    onOpenChange(true);
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>)
  {
    event.preventDefault();

    if (!canSubmit)
    {
      return;
    }

    try
    {
      await createOrganization.mutateAsync(
      {
        name: name.trim(),
        country,
        website: website.trim() || null,
      });

      closeDialog();
    }
    catch
    {
      // The mutation retains its error for display below.
    }
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent
        className="sm:max-w-md"
        showCloseButton={!createOrganization.isPending}
      >
        <DialogHeader>
          <DialogTitle>Create Organization</DialogTitle>

          <DialogDescription>
            Create a workspace for your team and its projects.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="grid gap-4">
          <div className="grid gap-2">
            <Label htmlFor="organization-name">
              Organization name
            </Label>

            <Input
              id="organization-name"
              value={name}
              onChange={(event) =>
              {
                setName(event.target.value);
              }}
              placeholder="My Organization"
              required
              disabled={createOrganization.isPending}
            />
          </div>

          <div className="grid gap-2">
            <Label htmlFor="organization-country">
              Country
            </Label>

            <CountryCombobox
              id="organization-country"
              value={country}
              onValueChange={setCountry}
              disabled={createOrganization.isPending}
            />
          </div>

          <div className="grid gap-2">
            <Label htmlFor="organization-website">
              Website
            </Label>

            <Input
              id="organization-website"
              type="url"
              value={website}
              onChange={(event) =>
            {
              setWebsite(event.target.value);
            }}
              placeholder="https://example.com"
              disabled={createOrganization.isPending}
            />
          </div>

          <div className="flex items-start gap-2 text-xs text-muted-foreground">
            <Info
              aria-hidden="true"
              className="mt-0.5 size-4 shrink-0"
            />

            <p>
              Your profile name (if provided) and account email address
              will be visible to members authorized to view this
              Organization&apos;s administration.
            </p>
          </div>

          {createOrganization.isError && (
            <p role="alert" className="text-sm text-destructive">
              Unable to create the organization. Please try again.
            </p>
          )}

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() =>
              {
                handleOpenChange(false);
              }}
              disabled={createOrganization.isPending}
            >
              Cancel
            </Button>

            <Button type="submit" disabled={!canSubmit}>
              {createOrganization.isPending
                ? "Creating..."
                : "Create Organization"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
