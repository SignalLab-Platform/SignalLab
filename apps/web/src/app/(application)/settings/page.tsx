import { AppearanceToggle } from "@/components/appearance/appearance-toggle";

export default function SettingsPage()
{
  return (
    <div className="mx-auto w-full max-w-4xl p-8">
      <div className="space-y-1">
        <h1 className="text-2xl font-semibold tracking-tight">
          Settings
        </h1>

        <p className="text-sm text-muted-foreground">
          Manage your SignalLab preferences.
        </p>
      </div>

      <section className="mt-8 border-t border-border pt-6">
        <div className="space-y-1">
          <h2 className="text-base font-semibold">
            Appearance
          </h2>

          <p className="text-sm text-muted-foreground">
            Choose how SignalLab looks on this device.
          </p>
        </div>

        <div className="mt-4">
          <AppearanceToggle />
        </div>
      </section>
    </div>
  );
}