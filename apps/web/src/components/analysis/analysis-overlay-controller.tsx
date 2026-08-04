"use client";

import
{
  ChartNoAxesCombined,
  X,
} from "lucide-react";

import
{
  useCallback,
  useEffect,
  useRef,
  useState,
} from "react";

import
{
  usePathname,
  useRouter,
  useSearchParams,
} from "next/navigation";

import { DEMO_NAVIGATION_DATA } from "@/navigation/demo-navigation-data";

import
{
  ANALYSIS_OVERLAY_QUERY_KEY,
  ANALYSIS_OVERLAY_QUERY_VALUE,
  buildAnalysisOverlayRoute,
  removeAnalysisOverlayFromRoute,
} from "@/navigation/navigation-routes";

type ProjectAnalysisContext =
{
  kind: "project";
  projectId: string;
};

type CampaignAnalysisContext =
{
  kind: "campaign";
  campaignId: string;
  localView: string;
};

type AnalysisOverlayContext =
  | ProjectAnalysisContext
  | CampaignAnalysisContext;

const ANALYSIS_TABS =
[
  {
    value: "subjects",
    label: "Subjects",
  },
  {
    value: "scope",
    label: "Scope",
  },
  {
    value: "groups",
    label: "Groups",
  },
  {
    value: "filters",
    label: "Filters",
  },
  {
    value: "visualization",
    label: "Visualization",
  },
  {
    value: "sources",
    label: "Sources",
  },
] as const;

type AnalysisTab =
  (typeof ANALYSIS_TABS)[number]["value"];

const FOCUSABLE_ELEMENT_SELECTOR =
  [
    "button:not([disabled])",
    "[href]",
    "input:not([disabled])",
    "select:not([disabled])",
    "textarea:not([disabled])",
    '[tabindex]:not([tabindex="-1"])',
  ].join(",");

function resolveAnalysisOverlayContext(
  pathname: string
): AnalysisOverlayContext | null
{
  const segments = pathname
    .split("/")
    .filter((segment) =>
    {
      return segment.length > 0;
    });

  const isProjectRoute =
    segments.length === 5 &&
    segments[0] === "organizations" &&
    segments[2] === "projects";

  if (isProjectRoute)
  {
    return {
      kind: "project",
      projectId: segments[3],
    };
  }

  const isCampaignRoute =
    segments.length === 7 &&
    segments[0] === "organizations" &&
    segments[2] === "projects" &&
    segments[4] === "campaigns";

  if (isCampaignRoute)
  {
    return {
      kind: "campaign",
      campaignId: segments[5],
      localView: segments[6],
    };
  }

  return null;
}

function getProjectLabel(projectId: string): string
{
  return projectId === DEMO_NAVIGATION_DATA.project.id
    ? DEMO_NAVIGATION_DATA.project.label
    : projectId;
}

function getCampaignLabel(campaignId: string): string
{
  return campaignId === DEMO_NAVIGATION_DATA.campaign.id
    ? DEMO_NAVIGATION_DATA.campaign.label
    : campaignId;
}

function getAnalysisOrigin(
  context: AnalysisOverlayContext
): string
{
  if (context.kind === "project")
  {
    return `Project / ${getProjectLabel(context.projectId)}`;
  }

  const contextType =
    context.localView === "results"
      ? "Campaign Results"
      : "Campaign";

  return `${contextType} / ${getCampaignLabel(context.campaignId)}`;
}

function getTabClassName(isActive: boolean): string
{
  const baseClassName =
    "inline-flex h-11 items-center border-b-2 px-1 text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2";

  if (isActive)
  {
    return `${baseClassName} border-primary text-foreground`;
  }

  return `${baseClassName} border-transparent text-muted-foreground hover:border-border hover:text-foreground`;
}

function getFocusableElements(
  container: HTMLElement
): HTMLElement[]
{
  return Array.from(
    container.querySelectorAll<HTMLElement>(
      FOCUSABLE_ELEMENT_SELECTOR
    )
  ).filter((element) =>
  {
    return (
      !element.hasAttribute("disabled") &&
      element.getAttribute("aria-hidden") !== "true"
    );
  });
}

export function AnalysisOverlayController()
{
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const router = useRouter();

  const analysisContext = resolveAnalysisOverlayContext(pathname);

  const [activeTab, setActiveTab] =
    useState<AnalysisTab>("subjects");

  const analyticsButtonRef =
    useRef<HTMLButtonElement>(null);

  const closeButtonRef =
    useRef<HTMLButtonElement>(null);

  const dialogRef =
    useRef<HTMLElement>(null);

  const wasOpenRef = useRef(false);
  const openedFromTriggerRef = useRef(false);

  const searchString = searchParams.toString();

  const currentRoute = searchString.length > 0
    ? `${pathname}?${searchString}`
    : pathname;

  const isAvailable = analysisContext !== null;

  const isOpen =
    isAvailable &&
    searchParams.get(
      ANALYSIS_OVERLAY_QUERY_KEY
    ) === ANALYSIS_OVERLAY_QUERY_VALUE;

  const origin =
  analysisContext !== null
    ? getAnalysisOrigin(analysisContext)
    : "";

  const activeTabDefinition =
    ANALYSIS_TABS.find((tab) =>
    {
      return tab.value === activeTab;
    }) ?? ANALYSIS_TABS[0];

  const closeAnalysisOverlay =
    useCallback(() =>
    {
      if (openedFromTriggerRef.current)
      {
        openedFromTriggerRef.current = false;
        router.back();

        return;
      }

      router.replace(
        removeAnalysisOverlayFromRoute(
          currentRoute
        ),
        {
          scroll: false,
        }
      );
    }, [currentRoute, router]);

  useEffect(() =>
  {
    if (!isOpen)
    {
      if (wasOpenRef.current)
      {
        wasOpenRef.current = false;
        openedFromTriggerRef.current = false;
        analyticsButtonRef.current?.focus();
      }

      return;
    }

    wasOpenRef.current = true;

    const scrollport = document.getElementById(
      "application-content-scrollport"
    );

    const previousOverflow =
      scrollport?.style.overflow ?? "";

    const previouslyInert =
      scrollport?.hasAttribute("inert") ?? false;

    if (scrollport !== null)
    {
      scrollport.style.overflow = "hidden";
      scrollport.setAttribute("inert", "");
    }

    closeButtonRef.current?.focus();

    function handleKeyDown(
      event: KeyboardEvent
    ): void
    {
      if (event.key === "Escape")
      {
        event.preventDefault();
        closeAnalysisOverlay();

        return;
      }

      if (
        event.key !== "Tab" ||
        dialogRef.current === null
      )
      {
        return;
      }

      const focusableElements =
        getFocusableElements(
          dialogRef.current
        );

      if (focusableElements.length === 0)
      {
        event.preventDefault();
        closeButtonRef.current?.focus();

        return;
      }

      const firstElement =
        focusableElements[0];

      const lastElement =
        focusableElements[
          focusableElements.length - 1
        ];

      const activeElement =
        document.activeElement;

      if (
        event.shiftKey &&
        activeElement === firstElement
      )
      {
        event.preventDefault();
        lastElement.focus();

        return;
      }

      if (
        !event.shiftKey &&
        activeElement === lastElement
      )
      {
        event.preventDefault();
        firstElement.focus();
      }
    }

    document.addEventListener(
      "keydown",
      handleKeyDown
    );

    return () =>
    {
      document.removeEventListener(
        "keydown",
        handleKeyDown
      );

      if (scrollport !== null)
      {
        scrollport.style.overflow =
          previousOverflow;

        if (!previouslyInert)
        {
          scrollport.removeAttribute(
            "inert"
          );
        }
      }
    };
  }, [
    closeAnalysisOverlay,
    isOpen,
  ]);

  function openAnalysisOverlay(): void
  {
    openedFromTriggerRef.current = true;

    router.push(
      buildAnalysisOverlayRoute(
        currentRoute
      ),
      {
        scroll: false,
      }
    );
  }

  function handleBackdropMouseDown(
    event: React.MouseEvent<HTMLDivElement>
  ): void
  {
    if (event.target !== event.currentTarget)
    {
      return;
    }

    event.preventDefault();
    closeAnalysisOverlay();
  }

  if (!isAvailable)
  {
    return null;
  }

  return (
    <>
      <button
        ref={analyticsButtonRef}
        type="button"
        aria-label="Analytics"
        aria-hidden={isOpen}
        tabIndex={isOpen ? -1 : 0}
        title="Open Analytics"
        onClick={openAnalysisOverlay}
        className={
          isOpen
            ? "pointer-events-none invisible absolute bottom-6 right-6"
            : "pointer-events-auto absolute bottom-6 right-6 z-10 inline-flex h-9 items-center gap-2 rounded-full bg-primary px-3 text-sm font-semibold text-primary-foreground shadow-lg transition-transform hover:scale-[1.02] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
        }
      >
        <ChartNoAxesCombined
          aria-hidden="true"
          className="size-4"
        />

        Analytics
      </button>

      {isOpen
        ? (
          <div
            data-analysis-overlay-backdrop
            onMouseDown={handleBackdropMouseDown}
            className="pointer-events-auto absolute inset-0 bg-background/50 backdrop-blur-[2px]"
          >
            <section
              ref={dialogRef}
              role="dialog"
              aria-modal="true"
              aria-labelledby="analysis-overlay-title"
              aria-describedby="analysis-overlay-description"
              className="absolute bottom-20 right-6 flex h-[85%] w-[82%] min-h-0 flex-col overflow-hidden rounded-2xl border border-border bg-card text-card-foreground shadow-2xl"
            >
              <header className="flex h-14 shrink-0 items-center justify-between gap-4 border-b border-border px-5">
                <div className="min-w-0">
                  <div className="flex min-w-0 items-center gap-2">
                    <span className="text-[0.65rem] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
                      Analysis
                    </span>

                    <span
                      aria-hidden="true"
                      className="text-muted-foreground"
                    >
                      /
                    </span>

                    <h1
                      id="analysis-overlay-title"
                      className="truncate text-sm font-semibold"
                    >
                      New Analysis
                    </h1>

                    <span className="shrink-0 rounded-full border bg-muted px-2 py-0.5 text-[0.65rem] font-medium text-muted-foreground">
                      Temporary
                    </span>
                  </div>

                  <p
                    id="analysis-overlay-description"
                    className="mt-0.5 truncate text-xs text-muted-foreground"
                  >
                    Initialized from {origin}
                  </p>
                </div>

                <button
                  ref={closeButtonRef}
                  type="button"
                  aria-label="Close Analysis"
                  title="Close Analysis"
                  onClick={
                    closeAnalysisOverlay
                  }
                  className="inline-flex size-9 shrink-0 items-center justify-center rounded-lg text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                >
                  <X
                    aria-hidden="true"
                    className="size-4"
                  />
                </button>
              </header>

              <nav
                aria-label="Analysis navigation"
                className="shrink-0 overflow-x-auto border-b border-border px-5"
              >
                <div
                  role="tablist"
                  aria-label="Analysis sections"
                  className="flex min-w-max items-center gap-5"
                >
                  {ANALYSIS_TABS.map((tab) =>
                  {
                    const isActive =
                      tab.value === activeTab;

                    return (
                      <button
                        key={tab.value}
                        id={`analysis-tab-${tab.value}`}
                        type="button"
                        role="tab"
                        aria-selected={isActive}
                        aria-controls="analysis-tab-panel"
                        onClick={() =>
                        {
                          setActiveTab(
                            tab.value
                          );
                        }}
                        className={
                          getTabClassName(
                            isActive
                          )
                        }
                      >
                        {tab.label}
                      </button>
                    );
                  })}
                </div>
              </nav>

              <div
                id="analysis-tab-panel"
                role="tabpanel"
                aria-labelledby={`analysis-tab-${activeTab}`}
                className="min-h-0 flex-1 overflow-y-auto p-6"
              >
                <div className="flex min-h-full items-center justify-center">
                  <div className="w-full max-w-xl rounded-xl border border-dashed bg-muted/20 p-8 text-center">
                    <p className="text-xs font-semibold uppercase tracking-[0.16em] text-primary">
                      {activeTabDefinition.label}
                    </p>

                    <h2 className="mt-2 text-xl font-semibold">
                      {activeTabDefinition.label} configuration
                    </h2>

                    <p className="mt-3 text-sm leading-6 text-muted-foreground">
                      This region establishes the Analysis Workspace
                      structure. Its analytical behavior will be implemented
                      in M09.
                    </p>
                  </div>
                </div>
              </div>
            </section>
          </div>
        )
        : null}
    </>
  );
}
