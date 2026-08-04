"use client";

import { useEffect } from "react";

import
{
  readNavigationState,
  updateNavigationResourceState,
} from "@/navigation/navigation-state";

import type
{
  NavigationLocation,
} from "@/navigation/navigation-types";

type NavigationStateRecorderProps =
{
  location: NavigationLocation;
};

export function NavigationStateRecorder(
  { location }: Readonly<NavigationStateRecorderProps>
)
{
  useEffect(() =>
  {
    if (location.localView === null)
    {
      return;
    }

    const currentView =
      readNavigationState()
        .resources[location.contextKey]
        ?.lastLocalView;

    if (currentView === location.localView)
    {
      return;
    }

    updateNavigationResourceState(
      location.contextKey,
      {
        lastLocalView: location.localView,
      }
    );
  }, [location.contextKey, location.localView]);

  return null;
}
