import { AppearanceToggle } from "@/components/appearance/appearance-toggle";
import { NotificationTrigger } from "@/components/notifications/notification-trigger";

export function AppHeader()
{
  return (
    <header className="flex min-w-0 items-center justify-between border-b border-border bg-shell-header px-5 text-shell-header-foreground">
      <div className="flex min-w-0 items-center gap-3">
        <span
          aria-hidden="true"
          className="size-2.5 shrink-0 rounded-full bg-primary"
        />

        <div className="min-w-0">
          <p className="truncate text-sm font-semibold tracking-tight">
            SignalLab
          </p>

          <p className="truncate text-xs text-muted-foreground">
            Game User Research platform
          </p>
        </div>
      </div>

      <div
        aria-label="Global controls"
        className="flex shrink-0 items-center gap-2"
      >
        <NotificationTrigger />
        <AppearanceToggle />
      </div>
    </header>
  );
}
