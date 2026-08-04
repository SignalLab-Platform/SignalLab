"use client";

import
{
  PanelLeftClose,
  PanelLeftOpen,
} from "lucide-react";

import { useState } from "react";

import { ContextNavigation } from "@/components/navigation/context-navigation";

import
{
  applySidebarState,
  readStoredSidebarState,
  storeSidebarState,
} from "@/lib/sidebar";

import type { SidebarState } from "@/lib/sidebar";

export function AppSidebar()
{
  const [state, setState] = useState<SidebarState>(readStoredSidebarState);

  const isCollapsed = state === "collapsed";
  const buttonLabel = isCollapsed ? "Expand sidebar" : "Collapse sidebar";

  function toggleSidebar(): void
  {
    const nextState = isCollapsed ? "expanded" : "collapsed";

    applySidebarState(nextState);
    storeSidebarState(nextState);
    setState(nextState);
  }

  return (
    <aside
      id="application-sidebar"
      aria-label="Application sidebar"
      className="min-h-0 min-w-0 overflow-hidden border-r border-sidebar-border bg-sidebar text-sidebar-foreground"
    >
      <div className="flex h-full min-h-0 flex-col">
        <div className="flex h-14 shrink-0 items-center justify-between border-b border-sidebar-border px-3">
          <p
            data-sidebar-expanded-only
            className="text-xs font-semibold uppercase tracking-[0.16em] text-muted-foreground"
          >
            Context
          </p>

          <button
            type="button"
            aria-controls="application-sidebar-content"
            aria-expanded={!isCollapsed}
            aria-label={buttonLabel}
            title={buttonLabel}
            suppressHydrationWarning
            onClick={toggleSidebar}
            className="inline-flex size-9 shrink-0 items-center justify-center rounded-lg text-muted-foreground transition-colors hover:bg-sidebar-accent hover:text-sidebar-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-sidebar-ring"
          >
            <PanelLeftOpen
              data-sidebar-collapsed-only
              aria-hidden="true"
              className="size-4"
            />

            <PanelLeftClose
              data-sidebar-expanded-only
              aria-hidden="true"
              className="size-4"
            />
          </button>
        </div>

        <div
          id="application-sidebar-content"
          className="min-h-0 flex-1 overflow-y-auto p-2"
        >
          <ContextNavigation />
        </div>
      </div>
    </aside>
  );
}
