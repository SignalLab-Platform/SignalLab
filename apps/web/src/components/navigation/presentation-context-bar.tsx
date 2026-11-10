type PresentationContextBarProps =
{
  variant: "explorer" | "workspace" | "document";
  breadcrumb: React.ReactNode;
  navigation: React.ReactNode;
  actions?: React.ReactNode;
};

export function PresentationContextBar(
  {
    variant,
    breadcrumb,
    navigation,
    actions,
  }: Readonly<PresentationContextBarProps>
)
{
  return (
    <div
      data-sticky-presentation-bar={variant}
      className="sticky top-0 z-20 border-b border-border bg-shell-content/95 backdrop-blur"
    >
      <div className="mx-auto w-full max-w-6xl px-8">
        <div className="flex h-11 min-w-0 items-center border-b border-border/60">
          {breadcrumb}
        </div>

        <div className="flex min-h-12 min-w-0 items-center justify-between gap-4">
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
    </div>
  );
}