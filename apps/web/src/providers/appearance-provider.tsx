"use client";

import
{
  createContext,
  useCallback,
  useContext,
  useMemo,
  useSyncExternalStore,
} from "react";

import
{
  AppearanceMode,
  readAppliedAppearanceMode,
  readServerAppearanceMode,
  setAppearanceMode,
  subscribeToAppearance,
} from "@/lib/appearance";

type AppearanceContextValue =
{
  mode: AppearanceMode;
  isDark: boolean;
  setMode: (mode: AppearanceMode) => void;
  toggleMode: () => void;
};

const AppearanceContext = createContext<AppearanceContextValue | undefined>(undefined);

export function AppearanceProvider(
{
  children,
}: Readonly<
{
  children: React.ReactNode;
}>)
{
  const mode = useSyncExternalStore(
    subscribeToAppearance,
    readAppliedAppearanceMode,
    readServerAppearanceMode
  );

  const setMode = useCallback((nextMode: AppearanceMode) =>
  {
    setAppearanceMode(nextMode);
  }, []);

  const toggleMode = useCallback(() =>
  {
    const currentMode = readAppliedAppearanceMode();
    const nextMode = currentMode === "dark" ? "light" : "dark";

    setAppearanceMode(nextMode);
  }, []);

  const value = useMemo<AppearanceContextValue>(() =>
  {
    return {
      mode,
      isDark: mode === "dark",
      setMode,
      toggleMode,
    };
  }, [mode, setMode, toggleMode]);

  return (
    <AppearanceContext.Provider value={value}>
      {children}
    </AppearanceContext.Provider>
  );
}

export function useAppearance(): AppearanceContextValue
{
  const context = useContext(AppearanceContext);

  if (context === undefined)
  {
    throw new Error("useAppearance must be used inside AppearanceProvider.");
  }

  return context;
}
