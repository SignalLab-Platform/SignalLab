import Link from "next/link";
import { ArrowLeft, ChevronRight } from "lucide-react";

import type { LucideIcon } from "lucide-react";

export type ContextBreadcrumbItem =
{
  label: string;
  href?: string;
  icon?: LucideIcon;
  action?: React.ReactNode;
};

type ContextBreadcrumbProps =
{
  items: readonly ContextBreadcrumbItem[];
  backHref: string;
};

export function ContextBreadcrumb(
  { items, backHref }: Readonly<ContextBreadcrumbProps>
)
{
  return (
    <div className="flex min-w-0 items-center gap-3">
      <Link
        href={backHref}
        scroll={false}
        aria-label="Go to parent context"
        className="inline-flex size-8 shrink-0 items-center justify-center rounded-md text-muted-foreground hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
      >
        <ArrowLeft aria-hidden="true" className="size-4" />
      </Link>

      <nav
        aria-label="Context breadcrumb"
        className="min-w-0 overflow-hidden"
      >
        <ol className="flex min-w-0 items-center gap-2">
          {items.map((item, index) =>
          {
            const Icon = item.icon;
            const isCurrent = index === items.length - 1;

            return (
              <li
                key={index}
                className="flex min-w-0 items-center gap-2"
              >
                {index > 0
                  ? (
                    <ChevronRight
                      aria-hidden="true"
                      className="size-4 shrink-0 text-muted-foreground"
                    />
                  )
                  : null}

                {Icon
                  ? (
                    <Icon
                      aria-hidden="true"
                      className="size-4 shrink-0 text-muted-foreground"
                    />
                  )
                  : null}

                {item.href && !isCurrent
                  ? (
                    <Link
                      href={item.href}
                      scroll={false}
                      title={item.label}
                      className="truncate text-sm text-muted-foreground hover:text-foreground hover:underline"
                    >
                      {item.label}
                    </Link>
                  )
                  : (
                    <h1
                      title={item.label}
                      aria-current={isCurrent ? "page" : undefined}
                      className="truncate text-sm font-semibold"
                    >
                      {item.label}
                    </h1>
                  )}

                {item.action !== undefined
                  ? (
                    <span className="flex shrink-0 items-center">
                      {item.action}
                    </span>
                  )
                  : null}
              </li>
            );
          })}
        </ol>
      </nav>
    </div>
  );
}