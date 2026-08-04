import { AppContent } from "@/components/shell/app-content";
import { AppHeader } from "@/components/shell/app-header";
import { AppSidebar } from "@/components/shell/app-sidebar";

type AppShellProps =
{
  children: React.ReactNode;
};

export function AppShell(
{
  children,
}: Readonly<AppShellProps>)
{
  return (
    <div className="grid h-dvh grid-rows-[4rem_minmax(0,1fr)] overflow-hidden bg-background">
      <AppHeader />

      <div className="grid min-h-0 min-w-0 grid-cols-[var(--application-sidebar-width)_minmax(0,1fr)]">
        <AppSidebar />

        <AppContent>
          {children}
        </AppContent>
      </div>
    </div>
  );
}
