import { Suspense } from "react";

import
{
  AnalysisOverlayController,
} from "@/components/analysis/analysis-overlay-controller";

type AppContentProps =
{
  children: React.ReactNode;
};

export function AppContent(
  {
    children,
  }: Readonly<AppContentProps>
)
{
  return (
    <main
      id="application-content"
      className="relative min-h-0 min-w-0 overflow-hidden bg-shell-content"
    >
      <div
        id="application-content-scrollport"
        className="h-full min-h-0 overflow-y-auto overscroll-contain"
      >
        {children}
      </div>

      <div
        id="application-content-overlay-root"
        className="pointer-events-none absolute inset-0 z-30"
      >
        <Suspense fallback={null}>
          <AnalysisOverlayController />
        </Suspense>
      </div>
    </main>
  );
}
