import { useAuth } from "@clerk/nextjs";
import { QueryClient, useQueryClient } from "@tanstack/react-query";
import { render } from "@testing-library/react";
import { useEffect } from "react";

import { QueryProvider } from "@/providers/query-provider";

jest.mock("@clerk/nextjs", () =>
({
  useAuth: jest.fn(),
}));

const mockedUseAuth = jest.mocked(useAuth);

const queryClients: QueryClient[] = [];

function QueryClientObserver()
{
  const queryClient = useQueryClient();

  useEffect(() =>
  {
    queryClients.push(queryClient);
  }, [queryClient]);

  return null;
}

function mockIdentity(userId: string | null, isLoaded = true)
{
  mockedUseAuth.mockReturnValue(
  {
    userId,
    isLoaded,
    isSignedIn: userId !== null,
  } as ReturnType<typeof useAuth>);
}

describe("QueryProvider", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();
    queryClients.length = 0;
  });

  it("preserves the query client for the same user", () =>
  {
    mockIdentity("user-alice");

    const { rerender } = render(
      <QueryProvider>
        <QueryClientObserver />
      </QueryProvider>
    );

    const firstClient = queryClients[0];

    rerender(
      <QueryProvider>
        <QueryClientObserver />
      </QueryProvider>
    );

    expect(queryClients.at(-1)).toBe(firstClient);
  });

  it("creates an empty cache when the authenticated user changes", () =>
  {
    mockIdentity("user-alice");

    const { rerender } = render(
      <QueryProvider>
        <QueryClientObserver />
      </QueryProvider>
    );

    const aliceClient = queryClients.at(-1)!;

    aliceClient.setQueryData(
      ["current-user", "notifications"],
      [{ id: "alice-notification" }]
    );

    mockIdentity("user-bob");

    rerender(
      <QueryProvider>
        <QueryClientObserver />
      </QueryProvider>
    );

    const bobClient = queryClients.at(-1)!;

    expect(bobClient).not.toBe(aliceClient);

    expect(
      bobClient.getQueryData(["current-user", "notifications"])
    ).toBeUndefined();
  });

  it("discards the authenticated cache on sign-out", () =>
  {
    mockIdentity("user-alice");

    const { rerender } = render(
      <QueryProvider>
        <QueryClientObserver />
      </QueryProvider>
    );

    const authenticatedClient = queryClients.at(-1)!;

    mockIdentity(null);

    rerender(
      <QueryProvider>
        <QueryClientObserver />
      </QueryProvider>
    );

    expect(queryClients.at(-1)).not.toBe(authenticatedClient);
  });

  it("creates a new cache when Clerk finishes loading", () =>
  {
    mockIdentity(null, false);

    const { rerender } = render(
      <QueryProvider>
        <QueryClientObserver />
      </QueryProvider>
    );

    const loadingClient = queryClients.at(-1)!;

    mockIdentity("user-alice");

    rerender(
      <QueryProvider>
        <QueryClientObserver />
      </QueryProvider>
    );

    expect(queryClients.at(-1)).not.toBe(loadingClient);
  });
});