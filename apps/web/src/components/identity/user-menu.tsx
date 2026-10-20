"use client";

import Image from "next/image";
import Link from "next/link";

import { useClerk } from "@clerk/nextjs";
import
{
  ChevronDown,
  LogOut,
  Settings,
  UserRound,
} from "lucide-react";
import { DropdownMenu } from "radix-ui";

import
{
  useCurrentUserProfile,
} from "@/features/current-user-profile/use-current-user-profile";

export function UserMenu()
{
  const { signOut } = useClerk();
  const profileQuery = useCurrentUserProfile();

  const avatarUrl = profileQuery.data?.avatarUrl ?? null;

  return (
    <DropdownMenu.Root>
      <DropdownMenu.Trigger
        className="flex h-9 items-center gap-2 rounded-md px-2 text-sm font-medium outline-none transition-colors hover:bg-shell-header-accent focus-visible:ring-2 focus-visible:ring-ring"
        aria-label="User menu"
      >
        <span className="flex size-7 overflow-hidden items-center justify-center rounded-full bg-primary text-xs font-semibold text-primary-foreground">
          {avatarUrl
            ? (
              <Image
                src={avatarUrl}
                alt=""
                width={28}
                height={28}
                unoptimized
                className="size-full object-cover"
              />
            )
            : (
              "U"
            )
          }
        </span>

        <ChevronDown
          aria-hidden="true"
          className="size-4 text-muted-foreground"
        />
      </DropdownMenu.Trigger>

      <DropdownMenu.Portal>
        <DropdownMenu.Content
          align="end"
          sideOffset={8}
          className="z-50 min-w-52 rounded-md border border-border bg-popover p-1 text-popover-foreground shadow-md"
        >
          <DropdownMenu.Item asChild>
            <Link
              href="/profile"
              className="flex cursor-default items-center gap-2 rounded-sm px-2 py-2 text-sm outline-none transition-colors focus:bg-accent focus:text-accent-foreground"
            >
              <UserRound
                aria-hidden="true"
                className="size-4"
              />

              My Profile
            </Link>
          </DropdownMenu.Item>

          <DropdownMenu.Item asChild>
            <Link
              href="/settings"
              className="flex cursor-default items-center gap-2 rounded-sm px-2 py-2 text-sm outline-none transition-colors focus:bg-accent focus:text-accent-foreground"
            >
              <Settings
                aria-hidden="true"
                className="size-4"
              />

              Settings
            </Link>
          </DropdownMenu.Item>

          <DropdownMenu.Separator className="my-1 h-px bg-border" />

          <DropdownMenu.Item
            className="flex cursor-default items-center gap-2 rounded-sm px-2 py-2 text-sm outline-none transition-colors focus:bg-accent focus:text-accent-foreground"
            onSelect={() =>
            {
              void signOut();
            }}
          >
            <LogOut
              aria-hidden="true"
              className="size-4"
            />

            Sign out
          </DropdownMenu.Item>
        </DropdownMenu.Content>
      </DropdownMenu.Portal>
    </DropdownMenu.Root>
  );
}