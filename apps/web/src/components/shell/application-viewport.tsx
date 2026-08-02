import { AppShell } from "@/components/shell/app-shell";
import { DesktopRequired } from "@/components/shell/desktop-required";

type ApplicationViewportProps =
{
  children: React.ReactNode;
};

export function ApplicationViewport({ children }: Readonly<ApplicationViewportProps>)
{
  return (
    <>
      <div data-desktop-application>
        <AppShell>
          {children}
        </AppShell>
      </div>

      <DesktopRequired />
    </>
  );
}
