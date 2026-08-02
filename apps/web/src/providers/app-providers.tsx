"use client";

import { AppearanceProvider } from "@/providers/appearance-provider";
import { QueryProvider } from "@/providers/query-provider";

export function AppProviders({ children }: Readonly<{ children: React.ReactNode; }>)
{
  return (
    <AppearanceProvider>
      <QueryProvider>
        {children}
      </QueryProvider>
    </AppearanceProvider>
  );
}
