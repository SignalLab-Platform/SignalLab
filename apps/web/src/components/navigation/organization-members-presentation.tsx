"use client";

import { useMemo } from "react";
import { MoreHorizontal, Search, UserPlus } from "lucide-react";

import
{
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

import
{
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

import { Button } from "@/components/ui/button";

import type
{
  OrganizationMember,
} from "@/features/organizations/organization-member";

export type MemberStatusFilter =
  | "active"
  | "removed"
  | "all";

export type MemberRoleFilter = string;

type OrganizationMembersActionsProps =
{
  searchQuery: string;
  onSearchQueryChange: (value: string) => void;
  roleFilter: MemberRoleFilter;
  onRoleFilterChange: (value: MemberRoleFilter) => void;
  statusFilter: MemberStatusFilter;
  onStatusFilterChange: (value: MemberStatusFilter) => void;
  roles: readonly string[];
};

type OrganizationMembersContentProps =
{
  members: readonly OrganizationMember[];
  searchQuery: string;
  roleFilter: MemberRoleFilter;
  statusFilter: MemberStatusFilter;
  isPending: boolean;
  isError: boolean;
  onRetry: () => void;
};

function getMemberDisplayName(member: OrganizationMember): string
{
  const name = [
    member.firstName?.trim(),
    member.lastName?.trim(),
  ]
    .filter(Boolean)
    .join(" ");

  return name || "Unnamed member";
}

function formatJoinedDate(value: string): string
{
  const date = new Date(value);

  if (Number.isNaN(date.getTime()))
  {
    return "Unknown";
  }

  return new Intl.DateTimeFormat("en-US",
  {
    year: "numeric",
    month: "short",
    day: "numeric",
  }).format(date);
}

export function OrganizationMembersActions(
  {
    searchQuery,
    onSearchQueryChange,
    roleFilter,
    onRoleFilterChange,
    statusFilter,
    onStatusFilterChange,
    roles,
  }: Readonly<OrganizationMembersActionsProps>
)
{
  return (
    <>
      <div className="relative w-52">
        <Search
          aria-hidden="true"
          className="pointer-events-none absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground"
        />

        <input
          type="search"
          value={searchQuery}
          onChange={(event) =>
          {
            onSearchQueryChange(event.target.value);
          }}
          aria-label="Search members"
          placeholder="Search members..."
          className="h-9 w-full rounded-lg border bg-background pl-9 pr-3 text-sm outline-none placeholder:text-muted-foreground focus-visible:ring-2 focus-visible:ring-ring"
        />
      </div>

      <Select
        value={roleFilter}
        onValueChange={onRoleFilterChange}
      >
        <SelectTrigger
          aria-label="Filter members by role"
          className="h-9 w-32 rounded-xl border-0 bg-muted/40 shadow-none focus-visible:ring-0 focus-visible:border-transparent"
        >
          <SelectValue placeholder="All roles" />
        </SelectTrigger>

        <SelectContent
          position="popper"
          side="bottom"
          sideOffset={6}
          align="start"
        >
          <SelectItem value="all">
            All roles
          </SelectItem>

          {roles.map((role) =>
          {
            return (
              <SelectItem key={role} value={role}>
                {role}
              </SelectItem>
            );
          })}
        </SelectContent>
      </Select>

      <Select
        value={statusFilter}
        onValueChange={(value) =>
        {
          onStatusFilterChange(value as MemberStatusFilter);
        }}
        >
        <SelectTrigger
          aria-label="Filter members by status"
          className="h-9 w-36 rounded-xl border-0 bg-muted/40 shadow-none focus-visible:ring-0 focus-visible:border-transparent"
        >
          <SelectValue placeholder="Active" />
        </SelectTrigger>

        <SelectContent
          position="popper"
          side="bottom"
          sideOffset={6}
          align="start"
        >
          <SelectItem value="active">
            Active
          </SelectItem>

          <SelectItem value="removed">
            Removed
          </SelectItem>

          <SelectItem value="all">
            All statuses
          </SelectItem>
        </SelectContent>
      </Select>

      <Button
        type="button"
        size="sm"
        disabled
        title="Member invitations will be introduced in SL-031."
      >
        <UserPlus aria-hidden="true" className="size-4" />
        Invite member
      </Button>
    </>
  );
}

export function OrganizationMembersContent(
  {
    members,
    searchQuery,
    roleFilter,
    statusFilter,
    isPending,
    isError,
    onRetry,
  }: Readonly<OrganizationMembersContentProps>
)
{
  const filteredMembers = useMemo(() =>
  {
    const normalizedSearch = searchQuery.trim().toLocaleLowerCase();

    return members.filter((member) =>
    {
      const memberStatus = member.status.toLowerCase();

      if (
        statusFilter !== "all" &&
        memberStatus !== statusFilter
      )
      {
        return false;
      }

      if (
        roleFilter !== "all" &&
        member.roleName !== roleFilter
      )
      {
        return false;
      }

      if (!normalizedSearch)
      {
        return true;
      }

      const searchableText = [
        member.firstName ?? "",
        member.lastName ?? "",
        getMemberDisplayName(member),
        member.email,
      ]
        .join(" ")
        .toLocaleLowerCase();

      return searchableText.includes(normalizedSearch);
    });
  }, [members, searchQuery, roleFilter, statusFilter]);

  if (isPending)
  {
    return (
      <div role="status" className="rounded-xl border bg-card p-8 text-sm text-muted-foreground">
        Loading organization members...
      </div>
    );
  }

  if (isError)
  {
    return (
      <div role="alert" className="rounded-xl border bg-card p-8">
        <p className="font-medium">
          Unable to load organization members.
        </p>

        <p className="mt-2 text-sm text-muted-foreground">
          Please try again.
        </p>

        <Button
          type="button"
          variant="outline"
          className="mt-4"
          onClick={onRetry}
        >
          Retry
        </Button>
      </div>
    );
  }

  return (
    <div className="overflow-hidden rounded-xl border bg-card">
      <div className="flex items-center justify-between border-b px-6 py-4">
        <p className="text-sm text-muted-foreground">
          {filteredMembers.length} of {members.length} members
        </p>
      </div>

      {filteredMembers.length === 0
        ? (
          <div className="p-10 text-center">
            <p className="font-medium">
              No members found
            </p>

            <p className="mt-2 text-sm text-muted-foreground">
              Try changing your search or filters.
            </p>
          </div>
        )
        : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm">
              <thead className="border-b bg-muted/30 text-xs text-muted-foreground">
                <tr>
                  <th scope="col" className="px-6 py-3 font-medium">
                    Member
                  </th>

                  <th scope="col" className="px-6 py-3 font-medium">
                    Role
                  </th>

                  <th scope="col" className="px-6 py-3 font-medium">
                    Status
                  </th>

                  <th scope="col" className="px-6 py-3 font-medium">
                    Joined
                  </th>

                  <th
                    scope="col"
                    className="w-16 px-6 py-3 text-right font-medium"
                  >
                    <span className="sr-only">
                      Actions
                    </span>
                  </th>
                </tr>
              </thead>

              <tbody className="divide-y">
                {filteredMembers.map((member) =>
                {
                  const isRemoved =
                    member.status.toLowerCase() === "removed";

                  return (
                    <tr key={member.id}>
                      <td className="px-6 py-4">
                        <div className="font-medium">
                          {getMemberDisplayName(member)}
                        </div>

                        <div className="mt-0.5 text-xs text-muted-foreground">
                          {member.email}
                        </div>
                      </td>

                      <td className="px-6 py-4">
                        {member.roleName}
                      </td>

                      <td className="px-6 py-4">
                        <span
                          className={
                            isRemoved
                              ? "text-muted-foreground"
                              : "text-foreground"
                          }
                        >
                          {member.status}
                        </span>
                      </td>

                      <td className="px-6 py-4 text-muted-foreground">
                        {formatJoinedDate(member.joinedAt)}
                      </td>

                      <td className="px-6 py-4 text-right">
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button
                              type="button"
                              variant="ghost"
                              size="icon"
                              aria-label={`Actions for ${getMemberDisplayName(member)}`}
                              className="size-8"
                            >
                              <MoreHorizontal
                                aria-hidden="true"
                                className="size-4"
                              />
                            </Button>
                          </DropdownMenuTrigger>

                          <DropdownMenuContent
                            align="end"
                            sideOffset={6}
                          >
                            <DropdownMenuItem disabled>
                              Change role
                            </DropdownMenuItem>

                            <DropdownMenuItem disabled>
                              {isRemoved
                              ? "Reactivate member"
                              : "Remove member"}
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
    </div>
  );
}