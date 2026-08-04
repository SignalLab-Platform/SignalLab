"use client";

import { useEffect } from "react";

import
{
  getNavigationScrollPosition,
  readNavigationState,
  updateNavigationScrollPosition,
} from "@/navigation/navigation-state";

import type
{
  NavigationLocation,
} from "@/navigation/navigation-types";

type NavigationScrollRestorationProps =
{
  location: NavigationLocation;
};

export function getNavigationScrollKey(
  location: NavigationLocation
): string
{
  if (location.contextKind === "participation")
  {
    return "document";
  }

  return location.localView ?? "default";
}

function getHashTarget(): HTMLElement | null
{
  if (
    typeof window === "undefined" ||
    window.location.hash.length <= 1
  )
  {
    return null;
  }

  try
  {
    const targetId = decodeURIComponent(
      window.location.hash.slice(1)
    );

    return document.getElementById(targetId);
  }
  catch
  {
    return null;
  }
}

export function NavigationScrollRestoration(
  {
    location,
  }: Readonly<NavigationScrollRestorationProps>
)
{
  const scrollKey = getNavigationScrollKey(location);

  useEffect(() =>
  {
    const scrollportElement = document.getElementById(
      "application-content-scrollport"
    );

    if (scrollportElement === null)
    {
      return;
    }

    const scrollport = scrollportElement;

    let lastKnownScrollTop = scrollport.scrollTop;
    let saveTimer: number | null = null;
    let hashRestoreTimer: number | null = null;

    function persistScrollPosition(): void
    {
      updateNavigationScrollPosition(
        location.contextKey,
        scrollKey,
        lastKnownScrollTop
      );
    }

    function restoreScrollPosition(): void
    {
      const hashTarget = getHashTarget();

      if (hashTarget !== null)
      {
        hashTarget.scrollIntoView(
        {
          block: "start",
        });

        lastKnownScrollTop = scrollport.scrollTop;

        return;
      }

      const storedPosition = getNavigationScrollPosition(
        readNavigationState(),
        location.contextKey,
        scrollKey
      );

      scrollport.scrollTop = storedPosition ?? 0;
      lastKnownScrollTop = scrollport.scrollTop;
    }

    function handleScroll(): void
    {
      lastKnownScrollTop = scrollport.scrollTop;

      if (saveTimer !== null)
      {
        return;
      }

      saveTimer = window.setTimeout(() =>
      {
        saveTimer = null;
        persistScrollPosition();
      }, 100);
    }

    function handleHashChange(): void
    {
      if (hashRestoreTimer !== null)
      {
        window.clearTimeout(hashRestoreTimer);
      }

      hashRestoreTimer = window.setTimeout(() =>
      {
        hashRestoreTimer = null;
        restoreScrollPosition();
      }, 0);
    }

    restoreScrollPosition();

    scrollport.addEventListener(
      "scroll",
      handleScroll,
      {
        passive: true,
      }
    );

    window.addEventListener(
      "hashchange",
      handleHashChange
    );

    return () =>
    {
      scrollport.removeEventListener(
        "scroll",
        handleScroll
      );

      window.removeEventListener(
        "hashchange",
        handleHashChange
      );

      if (saveTimer !== null)
      {
        window.clearTimeout(saveTimer);
      }

      if (hashRestoreTimer !== null)
      {
        window.clearTimeout(hashRestoreTimer);
      }

      persistScrollPosition();
    };
  }, [location.contextKey, scrollKey]);

  return null;
}
