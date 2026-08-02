"use client";

import
{
  Moon,
  Sun,
} from "lucide-react";

import { useAppearance } from "@/providers/appearance-provider";

export function AppearanceToggle()
{
  const { isDark, toggleMode } = useAppearance();

  const accessibleLabel = isDark ? "Switch to light mode" : "Switch to dark mode";

  return (
    <button
      type="button"
      aria-label={accessibleLabel}
      aria-pressed={isDark}
      title={accessibleLabel}
      onClick={toggleMode}
      className="inline-flex h-9 items-center gap-2 rounded-lg border border-border bg-background px-3 text-sm font-medium text-foreground shadow-sm transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 focus-visible:ring-offset-shell-header"
    >
      <Sun
        data-appearance-light-only
        aria-hidden="true"
        className="size-4"
      />

      <Moon
        data-appearance-dark-only
        aria-hidden="true"
        className="size-4"
      />

      <span data-appearance-light-only>
        Light
      </span>

      <span data-appearance-dark-only>
        Dark
      </span>
    </button>
  );
}
