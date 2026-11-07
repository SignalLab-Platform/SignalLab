import type { LucideIcon } from "lucide-react";

type PresentationContextBarVariant =
  | "explorer"
  | "workspace"
  | "document";

type PresentationContextBarProps =
{
  variant: PresentationContextBarVariant;
  contextType: string;
  contextLabel: string;
  contextIcon: LucideIcon;
  navigation: React.ReactNode;
  actions?: React.ReactNode;
};

export function PresentationContextBar(
  {
    variant,
    contextType,
    contextLabel,
    contextIcon: ContextIcon,
    navigation,
    actions,
  }: Readonly<PresentationContextBarProps>
)
{
  return (
    <div
      data-sticky-presentation-bar={variant}
      className="sticky top-0 z-20 h-14 border-b border-border bg-shell-content/95 backdrop-blur"
    >
      <div className="mx-auto flex h-full w-full max-w-6xl items-center gap-2 px-6">
        <div className="flex min-w-24 flex-1 items-center gap-2">
          <ContextIcon
            aria-hidden="true"
            className="size-4 shrink-0 text-muted-foreground"
          />

          <h1
            title={contextLabel}
            aria-label={`${contextType}: ${contextLabel}`}
            className="min-w-0 truncate whitespace-nowrap text-sm font-semibold"
          >
            {contextLabel}
          </h1>
        </div>

        <div className="min-w-0 flex-[0_1_auto]">
          {navigation}
        </div>

        {actions !== undefined
          ? (
            <div className="flex shrink-0 items-center gap-2">
              {actions}
            </div>
          )
          : null}
      </div>
    </div>
  );
}