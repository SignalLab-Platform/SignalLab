"use client";

import { PanelLeftClose, PanelLeftOpen } from "lucide-react";
import { useState } from "react";

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
    <nav
      id="application-sidebar"
      aria-label="Context navigation"
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
          data-sidebar-expanded-only
          className="min-h-0 overflow-y-auto p-4"
        >
          <div className="rounded-lg border border-dashed border-sidebar-border bg-sidebar-accent/40 p-4">
            <p className="text-sm font-medium">
              No context selected
            </p>

            <p className="mt-1 text-sm leading-6 text-muted-foreground">
              Context navigation will appear here when an application area is available.
            </p>
          </div>
        </div>
      </div>
    </nav>
  );
}
