"use client";

import
{
  PanelLeftClose,
  PanelLeftOpen,
} from "lucide-react";

import { useSyncExternalStore } from "react";

import { ContextNavigation } from "@/components/navigation/context-navigation";

import
{
  applySidebarState,
  getServerSidebarState,
  notifySidebarStateChanged,
  readStoredSidebarState,
  storeSidebarState,
  subscribeSidebarState,
} from "@/lib/sidebar";

export function AppSidebar()
{
  const state = useSyncExternalStore(
    subscribeSidebarState,
    readStoredSidebarState,
    getServerSidebarState
  );

  const isCollapsed = state === "collapsed";
  const buttonLabel = isCollapsed ? "Expand sidebar" : "Collapse sidebar";

  function toggleSidebar(): void
  {
    const nextState = isCollapsed ? "expanded" : "collapsed";

    applySidebarState(nextState);
    storeSidebarState(nextState);
    notifySidebarStateChanged();
  }

  return (
    <aside
      id="application-sidebar"
      aria-label="Application sidebar"
      className="min-h-0 min-w-0 overflow-hidden border-r border-sidebar-border bg-sidebar text-sidebar-foreground"
    >
      <div className="flex h-full min-h-0 flex-col">
        <div
          data-sidebar-header
          className="flex h-14 shrink-0 items-center justify-end border-b border-sidebar-border px-3"
        >
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
          className="min-h-0 flex-1 overflow-y-auto p-2"
        >
          <ContextNavigation />
        </div>
      </div>
    </aside>
  );
}