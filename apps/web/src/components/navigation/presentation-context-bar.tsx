type PresentationContextBarVariant =
  | "explorer"
  | "workspace"
  | "document";

type PresentationContextBarProps =
{
  variant: PresentationContextBarVariant;
  contextType: string;
  contextLabel: string;
  navigation: React.ReactNode;
  actions?: React.ReactNode;
};

export function PresentationContextBar(
  {
    variant,
    contextType,
    contextLabel,
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
      <div className="mx-auto flex h-full w-full max-w-6xl items-center gap-4 px-8">
        <div className="flex min-w-0 shrink-0 items-center gap-2">
          <span className="text-[0.65rem] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
            {contextType}
          </span>

          <span
            aria-hidden="true"
            className="text-muted-foreground"
          >
            /
          </span>

          <h1
            title={contextLabel}
            className="max-w-56 truncate text-sm font-semibold"
          >
            {contextLabel}
          </h1>
        </div>

        <div className="min-w-0 flex-1">
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
