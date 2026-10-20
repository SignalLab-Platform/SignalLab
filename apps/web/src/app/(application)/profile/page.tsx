"use client";

import Image from "next/image";
import
{
  useRef,
  useState,
} from "react";

import { Button } from "@/components/ui/button";
import
{
  useChangeCurrentUserAvatar,
} from "@/features/current-user-profile/use-change-current-user-avatar";
import
{
  ProfileEmailEditor,
} from "@/features/current-user-profile/profile-email-editor";
import
{
  useChangeCurrentUserFirstName,
} from "@/features/current-user-profile/use-change-current-user-first-name";
import
{
  useChangeCurrentUserLastName,
} from "@/features/current-user-profile/use-change-current-user-last-name";
import
{
  useCurrentUserProfile,
} from "@/features/current-user-profile/use-current-user-profile";

export default function ProfilePage()
{
  const profileQuery = useCurrentUserProfile();

  if (profileQuery.isPending)
  {
    return (
      <ProfilePageLayout>
        <p className="text-sm text-muted-foreground">
          Loading profile...
        </p>
      </ProfilePageLayout>
    );
  }

  if (profileQuery.isError)
  {
    return (
      <ProfilePageLayout>
        <p className="text-sm text-destructive">
          Unable to load your profile.
        </p>
      </ProfilePageLayout>
    );
  }

  return (
    <ProfileContent profile={profileQuery.data} />
  );
}

type ProfileContentProps = Readonly<
{
  profile:
  {
    email: string;
    firstName: string | null;
    lastName: string | null;
    language: string;
    avatarUrl: string | null;
  };
}>;

function ProfileContent(
{
  profile,
}: ProfileContentProps)
{
  const changeAvatar = useChangeCurrentUserAvatar();
  const changeFirstName = useChangeCurrentUserFirstName();
  const changeLastName = useChangeCurrentUserLastName();

  const avatarInputRef = useRef<HTMLInputElement>(null);

  const [isEditingName, setIsEditingName] = useState(false);
  const [firstName, setFirstName] = useState(
    profile.firstName ?? "",
  );
  const [lastName, setLastName] = useState(
    profile.lastName ?? "",
  );

  const isSaving =
    changeFirstName.isPending ||
    changeLastName.isPending;

  const hasSaveError =
    changeFirstName.isError ||
    changeLastName.isError;

  function handleEditName()
  {
    setFirstName(profile.firstName ?? "");
    setLastName(profile.lastName ?? "");
    setIsEditingName(true);
  }

  async function handleSave()
  {
    const normalizedFirstName = normalizeOptionalText(firstName);
    const normalizedLastName = normalizeOptionalText(lastName);

    const mutations: Promise<unknown>[] = [];

    if (normalizedFirstName !== profile.firstName)
    {
      mutations.push(
        changeFirstName.mutateAsync(normalizedFirstName),
      );
    }

    if (normalizedLastName !== profile.lastName)
    {
      mutations.push(
        changeLastName.mutateAsync(normalizedLastName),
      );
    }

    try
    {
      await Promise.all(mutations);
      setIsEditingName(false);
    }
    catch
    {
      // The mutation state renders the error below.
    }
  }

  function handleCancelName()
  {
    setIsEditingName(false);
  }

  function handleAvatarClick()
  {
    if (!changeAvatar.isPending)
    {
      avatarInputRef.current?.click();
    }
  }

  async function handleAvatarChange(
    event: React.ChangeEvent<HTMLInputElement>,
  )
  {
    const file = event.target.files?.[0];

    if (!file)
    {
      return;
    }

    try
    {
      await changeAvatar.mutateAsync(file);
    }
    catch
    {
      // The mutation state renders the error below.
    }
    finally
    {
      event.target.value = "";
    }
  }

  const initials = getInitials(
    profile.firstName,
    profile.lastName,
    profile.email,
  );

  return (
    <ProfilePageLayout>
      <section className="flex items-center gap-5 border-t border-border pt-6">
        <div className="relative size-20 shrink-0">
          <button
            type="button"
            onClick={handleAvatarClick}
            disabled={changeAvatar.isPending}
            aria-label="Change profile picture"
            className="group relative flex size-20 overflow-hidden rounded-full bg-primary text-xl font-semibold text-primary-foreground outline-none transition-opacity hover:opacity-90 focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-wait disabled:opacity-60"
          >
            {profile.avatarUrl
              ? (
                <Image
                  src={profile.avatarUrl}
                  alt=""
                  width={80}
                  height={80}
                  unoptimized
                  className="size-full object-cover"
                />
              )
              : (
                <span className="flex size-full items-center justify-center">
                  {initials}
                </span>
              )
            }

            <span
              aria-hidden="true"
              className="absolute bottom-0 right-0 flex size-7 items-center justify-center rounded-full border-2 border-background bg-foreground text-background shadow-sm"
            >
              <svg
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
                className="size-3.5"
              >
                <path d="M12 20h9" />
                <path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4Z" />
              </svg>
            </span>
          </button>

          <input
            ref={avatarInputRef}
            type="file"
            accept="image/png,image/jpeg,image/webp"
            onChange={handleAvatarChange}
            className="hidden"
          />
        </div>

        <div className="min-w-0">
          <p className="text-lg font-semibold">
            {getDisplayName(
              profile.firstName,
              profile.lastName,
            )}
          </p>

          <p className="truncate text-sm text-muted-foreground">
            {profile.email}
          </p>
        </div>
      </section>

      {changeAvatar.isError &&
        <p className="mt-3 text-sm text-destructive">
          Unable to change your profile picture.
        </p>
      }

      <section className="mt-8 border-t border-border pt-6">
        <div className="space-y-1">
          <h2 className="text-base font-semibold">
            Personal information
          </h2>

          <p className="text-sm text-muted-foreground">
            Your personal information associated with SignalLab.
          </p>
        </div>

        <div className="mt-6 divide-y divide-border">
          <div className="py-5 first:pt-0">
            {isEditingName
              ? (
                <div>
                  <div className="grid gap-6 sm:grid-cols-2">
                    <ProfileInput
                      id="first-name"
                      label="First name"
                      value={firstName}
                      disabled={isSaving}
                      onChange={setFirstName}
                    />

                    <ProfileInput
                      id="last-name"
                      label="Last name"
                      value={lastName}
                      disabled={isSaving}
                      onChange={setLastName}
                    />
                  </div>

                  {hasSaveError &&
                    <p className="mt-4 text-sm text-destructive">
                      Unable to save your personal information.
                    </p>
                  }

                  <div className="mt-4 flex gap-2">
                    <Button
                      onClick={handleSave}
                      disabled={isSaving}
                    >
                      {isSaving ? "Saving..." : "Save"}
                    </Button>

                    <Button
                      variant="outline"
                      onClick={handleCancelName}
                      disabled={isSaving}
                    >
                      Cancel
                    </Button>
                  </div>
                </div>
              )
              : (
                <div className="flex items-start justify-between gap-6">
                  <dl className="grid min-w-0 flex-1 gap-6 sm:grid-cols-2">
                    <ProfileField
                      label="First name"
                      value={profile.firstName}
                    />

                    <ProfileField
                      label="Last name"
                      value={profile.lastName}
                    />
                  </dl>

                  <Button
                    variant="outline"
                    onClick={handleEditName}
                  >
                    Edit
                  </Button>
                </div>
              )
            }
          </div>

          <ProfileEmailEditor email={profile.email} />

          <div className="flex items-start justify-between gap-6 py-5">
            <dl className="min-w-0 flex-1">
              <ProfileField
                label="Language"
                value={profile.language}
              />
            </dl>
          </div>
        </div>
      </section>
    </ProfilePageLayout>
  );
}

function ProfilePageLayout(
{
  children,
}: Readonly<
{
  children: React.ReactNode;
}>)
{
  return (
    <div className="mx-auto w-full max-w-4xl p-8">
      <div className="space-y-1">
        <h1 className="text-2xl font-semibold tracking-tight">
          My Profile
        </h1>

        <p className="text-sm text-muted-foreground">
          Manage your personal information and profile picture.
        </p>
      </div>

      <div className="mt-8">
        {children}
      </div>
    </div>
  );
}

function ProfileField(
{
  label,
  value,
}: Readonly<
{
  label: string;
  value: string | null;
}>)
{
  return (
    <div className="space-y-1">
      <dt className="text-sm font-medium">
        {label}
      </dt>

      <dd className="text-sm text-muted-foreground">
        {value ?? "Not set"}
      </dd>
    </div>
  );
}

function ProfileInput(
{
  id,
  label,
  value,
  disabled,
  onChange,
}: Readonly<
{
  id: string;
  label: string;
  value: string;
  disabled: boolean;
  onChange: (value: string) => void;
}>)
{
  return (
    <div className="space-y-2">
      <label
        htmlFor={id}
        className="text-sm font-medium"
      >
        {label}
      </label>

      <input
        id={id}
        type="text"
        value={value}
        disabled={disabled}
        onChange={(event) => onChange(event.target.value)}
        className="h-9 w-full rounded-md border border-input bg-background px-3 text-sm outline-none transition-colors focus-visible:border-ring focus-visible:ring-2 focus-visible:ring-ring/50 disabled:cursor-not-allowed disabled:opacity-50"
      />
    </div>
  );
}

function normalizeOptionalText(value: string)
{
  const normalizedValue = value.trim();

  return normalizedValue || null;
}

function getDisplayName(
  firstName: string | null,
  lastName: string | null,
)
{
  const displayName = [firstName, lastName]
    .filter(Boolean)
    .join(" ");

  return displayName || "SignalLab user";
}

function getInitials(
  firstName: string | null,
  lastName: string | null,
  email: string,
)
{
  const initials = [firstName, lastName]
    .filter(Boolean)
    .map((value) => value![0])
    .join("")
    .toUpperCase();

  if (initials)
  {
    return initials;
  }

  return email[0]?.toUpperCase() ?? "?";
}