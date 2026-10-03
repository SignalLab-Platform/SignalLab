import { Bell } from "lucide-react";

export function NotificationTrigger()
{
  return (
    <button
      aria-label="Notifications"
      className="inline-flex size-9 items-center justify-center rounded-md text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
      title="Notifications"
      type="button"
    >
      <Bell
        aria-hidden="true"
        className="size-4"
      />
    </button>
  );
}