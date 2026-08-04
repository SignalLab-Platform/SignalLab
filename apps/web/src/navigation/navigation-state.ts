"use client";

import { useSyncExternalStore } from "react";

import type
{
  NavigationContextKey,
} from "@/navigation/navigation-types";

type NavigationStateListener = () => void;

export type NavigationResourceState =
{
  lastLocalView?: string;
  explorerSearchQuery?: string;
  scrollPositions?: Record<string, number>;
};

export type NavigationState =
{
  version: 1;
  resources: Partial<
    Record<NavigationContextKey, NavigationResourceState>
  >;
};

export const NAVIGATION_STATE_STORAGE_KEY =
  "signallab.navigation.state.v1";

const EMPTY_NAVIGATION_STATE: NavigationState =
{
  version: 1,
  resources: {},
};

const navigationStateListeners = new Set<NavigationStateListener>();

let cachedNavigationState: NavigationState | null = null;

function isRecord(value: unknown): value is Record<string, unknown>
{
  return typeof value === "object" && value !== null;
}

function parseScrollPositions(
  value: unknown
): Record<string, number> | undefined
{
  if (!isRecord(value))
  {
    return undefined;
  }

  const scrollPositions: Record<string, number> = {};

  for (const [key, position] of Object.entries(value))
  {
    if (
      typeof position === "number" &&
      Number.isFinite(position) &&
      position >= 0
    )
    {
      scrollPositions[key] = position;
    }
  }

  return Object.keys(scrollPositions).length > 0
    ? scrollPositions
    : undefined;
}

function parseNavigationResourceState(
  value: unknown
): NavigationResourceState | null
{
  if (!isRecord(value))
  {
    return null;
  }

  const resourceState: NavigationResourceState = {};

  if (typeof value.lastLocalView === "string")
  {
    resourceState.lastLocalView = value.lastLocalView;
  }

  if (typeof value.explorerSearchQuery === "string")
  {
    resourceState.explorerSearchQuery = value.explorerSearchQuery;
  }

  const scrollPositions = parseScrollPositions(value.scrollPositions);

  if (scrollPositions !== undefined)
  {
    resourceState.scrollPositions = scrollPositions;
  }

  return resourceState;
}

function readStoredNavigationState(): NavigationState
{
  if (typeof window === "undefined")
  {
    return EMPTY_NAVIGATION_STATE;
  }

  try
  {
    const storedValue = window.localStorage.getItem(
      NAVIGATION_STATE_STORAGE_KEY
    );

    if (storedValue === null)
    {
      return EMPTY_NAVIGATION_STATE;
    }

    const parsedValue: unknown = JSON.parse(storedValue);

    if (
      !isRecord(parsedValue) ||
      parsedValue.version !== 1 ||
      !isRecord(parsedValue.resources)
    )
    {
      return EMPTY_NAVIGATION_STATE;
    }

    const resources: NavigationState["resources"] = {};

    for (
      const [contextKey, resourceValue]
      of Object.entries(parsedValue.resources)
    )
    {
      const resourceState =
        parseNavigationResourceState(resourceValue);

      if (resourceState !== null)
      {
        resources[contextKey as NavigationContextKey] =
          resourceState;
      }
    }

    return {
      version: 1,
      resources,
    };
  }
  catch
  {
    return EMPTY_NAVIGATION_STATE;
  }
}

function storeNavigationState(state: NavigationState): void
{
  if (typeof window === "undefined")
  {
    return;
  }

  try
  {
    window.localStorage.setItem(
      NAVIGATION_STATE_STORAGE_KEY,
      JSON.stringify(state)
    );
  }
  catch
  {
    return;
  }
}

function notifyNavigationStateListeners(): void
{
  navigationStateListeners.forEach((listener) =>
  {
    listener();
  });
}

function subscribeToNavigationState(
  listener: NavigationStateListener
): () => void
{
  navigationStateListeners.add(listener);

  return () =>
  {
    navigationStateListeners.delete(listener);
  };
}

function getNavigationStateSnapshot(): NavigationState
{
  if (cachedNavigationState === null)
  {
    cachedNavigationState = readStoredNavigationState();
  }

  return cachedNavigationState;
}

function getServerNavigationStateSnapshot(): NavigationState
{
  return EMPTY_NAVIGATION_STATE;
}

export function useNavigationState(): NavigationState
{
  return useSyncExternalStore(
    subscribeToNavigationState,
    getNavigationStateSnapshot,
    getServerNavigationStateSnapshot
  );
}

export function readNavigationState(): NavigationState
{
  return getNavigationStateSnapshot();
}

export function updateNavigationResourceState(
  contextKey: NavigationContextKey,
  patch: Partial<NavigationResourceState>
): void
{
  const currentState = getNavigationStateSnapshot();
  const currentResourceState =
    currentState.resources[contextKey] ?? {};

  const nextState: NavigationState =
  {
    version: 1,

    resources:
    {
      ...currentState.resources,

      [contextKey]:
      {
        ...currentResourceState,
        ...patch,
      },
    },
  };

  cachedNavigationState = nextState;

  storeNavigationState(nextState);
  notifyNavigationStateListeners();
}

export function updateNavigationScrollPosition(
  contextKey: NavigationContextKey,
  scrollKey: string,
  position: number
): void
{
  const normalizedPosition = Math.max(
    0,
    Math.round(position)
  );

  const currentState = getNavigationStateSnapshot();

  const currentResourceState =
    currentState.resources[contextKey] ?? {};

  const currentScrollPositions =
    currentResourceState.scrollPositions ?? {};

  if (
    currentScrollPositions[scrollKey] ===
    normalizedPosition
  )
  {
    return;
  }

  const nextState: NavigationState =
  {
    version: 1,

    resources:
    {
      ...currentState.resources,

      [contextKey]:
      {
        ...currentResourceState,

        scrollPositions:
        {
          ...currentScrollPositions,
          [scrollKey]: normalizedPosition,
        },
      },
    },
  };

  cachedNavigationState = nextState;

  storeNavigationState(nextState);
}

export function getNavigationScrollPosition(
  state: NavigationState,
  contextKey: NavigationContextKey,
  scrollKey: string
): number | undefined
{
  return state
    .resources[contextKey]
    ?.scrollPositions
    ?.[scrollKey];
}

export function getPersistedLocalView<TView extends string>(
  state: NavigationState,
  contextKey: NavigationContextKey,
  allowedViews: readonly TView[],
  fallbackView: TView
): TView
{
  const storedView =
    state.resources[contextKey]?.lastLocalView;

  const isAllowed = allowedViews.some((allowedView) =>
  {
    return allowedView === storedView;
  });

  return isAllowed
    ? storedView as TView
    : fallbackView;
}

export function resetNavigationState(): void
{
  cachedNavigationState = EMPTY_NAVIGATION_STATE;

  if (typeof window !== "undefined")
  {
    try
    {
      window.localStorage.removeItem(
        NAVIGATION_STATE_STORAGE_KEY
      );
    }
    catch
    {
      return;
    }
  }

  notifyNavigationStateListeners();
}
