import { MonitorUp } from "lucide-react";

export function DesktopRequired()
{
  return (
    <main
      data-desktop-required
      className="min-h-dvh place-items-center overflow-y-auto bg-background px-6 py-10 text-foreground"
    >
      <section
        aria-labelledby="desktop-required-title"
        className="w-full max-w-lg rounded-2xl border bg-card p-8 text-card-foreground shadow-sm"
      >
        <div className="mb-6 flex size-12 items-center justify-center rounded-xl bg-primary/15 text-primary">
          <MonitorUp
            aria-hidden="true"
            className="size-6"
          />
        </div>

        <p className="text-sm font-medium text-primary">
          SignalLab
        </p>

        <h1
          id="desktop-required-title"
          className="mt-2 text-3xl font-semibold tracking-tight"
        >
          Desktop browser required
        </h1>

        <p className="mt-4 text-base leading-7 text-muted-foreground">
          SignalLab is designed exclusively for desktop use.
        </p>

        <p className="mt-3 text-sm leading-6 text-muted-foreground">
          Open this page on a computer with a mouse or trackpad. If you are
          already using a computer, enlarge the browser window.
        </p>
      </section>
    </main>
  );
}
