export type SidebarState = "expanded" | "collapsed";

export const DEFAULT_SIDEBAR_STATE: SidebarState = "expanded";

export const SIDEBAR_STORAGE_KEY = "signallab.shell.sidebar";

export const SIDEBAR_INITIALIZER_SCRIPT = `
(() =>
{
  try
  {
    const storedState = window.localStorage.getItem("${SIDEBAR_STORAGE_KEY}");
    const state = storedState === "collapsed" ? "collapsed" : "expanded";

    document.documentElement.dataset.sidebarState = state;
  }
  catch
  {
    document.documentElement.dataset.sidebarState = "expanded";
  }
})();
`;

export function isSidebarState(value: string | null): value is SidebarState
{
  return value === "expanded" || value === "collapsed";
}

export function readStoredSidebarState(): SidebarState
{
  if (typeof window === "undefined")
  {
    return DEFAULT_SIDEBAR_STATE;
  }

  try
  {
    const storedState = window.localStorage.getItem(SIDEBAR_STORAGE_KEY);

    return isSidebarState(storedState) ? storedState : DEFAULT_SIDEBAR_STATE;
  }
  catch
  {
    return DEFAULT_SIDEBAR_STATE;
  }
}

export function storeSidebarState(state: SidebarState): void
{
  if (typeof window === "undefined")
  {
    return;
  }

  try
  {
    window.localStorage.setItem(SIDEBAR_STORAGE_KEY, state);
  }
  catch
  {
    return;
  }
}

export function applySidebarState(state: SidebarState): void
{
  if (typeof document === "undefined")
  {
    return;
  }

  document.documentElement.dataset.sidebarState = state;
}
