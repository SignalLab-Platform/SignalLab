"use client";

import { useAuth } from "@clerk/nextjs";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useState } from "react";

type QueryProviderProps = Readonly<
{
  children: React.ReactNode;
}>;

function SessionQueryProvider({ children }: QueryProviderProps)
{
  const [queryClient] = useState(() => new QueryClient());

  return (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );
}

export function QueryProvider({ children }: QueryProviderProps)
{
  const { isLoaded, userId } = useAuth();

  const sessionKey = !isLoaded
    ? "loading"
    : userId ?? "signed-out";

  return (
    <SessionQueryProvider key={sessionKey}>
      {children}
    </SessionQueryProvider>
  );
}