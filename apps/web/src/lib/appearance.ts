export type AppearanceMode = "light" | "dark";

type AppearanceListener = () => void;

export const DEFAULT_APPEARANCE_MODE: AppearanceMode = "light";

export const APPEARANCE_STORAGE_KEY = "signallab.appearance.mode";

const appearanceListeners = new Set<AppearanceListener>();

export const APPEARANCE_INITIALIZER_SCRIPT = `
(() =>
{
  try
  {
    const storedMode = window.localStorage.getItem("${APPEARANCE_STORAGE_KEY}");
    const mode = storedMode === "dark" ? "dark" : "light";
    const root = document.documentElement;

    root.classList.toggle("dark", mode === "dark");
    root.dataset.appearanceMode = mode;
  }
  catch
  {
    const root = document.documentElement;

    root.classList.remove("dark");
    root.dataset.appearanceMode = "light";
  }
})();
`;

export function isAppearanceMode(value: string | null): value is AppearanceMode
{
  return value === "light" || value === "dark";
}

export function readStoredAppearanceMode(): AppearanceMode
{
  if (typeof window === "undefined")
  {
    return DEFAULT_APPEARANCE_MODE;
  }

  try
  {
    const storedMode = window.localStorage.getItem(APPEARANCE_STORAGE_KEY);

    return isAppearanceMode(storedMode) ? storedMode : DEFAULT_APPEARANCE_MODE;
  }
  catch
  {
    return DEFAULT_APPEARANCE_MODE;
  }
}

export function readAppliedAppearanceMode(): AppearanceMode
{
  if (typeof document === "undefined")
  {
    return DEFAULT_APPEARANCE_MODE;
  }

  const appliedMode = document.documentElement.dataset.appearanceMode ?? null;

  return isAppearanceMode(appliedMode) ? appliedMode : readStoredAppearanceMode();
}

export function readServerAppearanceMode(): AppearanceMode
{
  return DEFAULT_APPEARANCE_MODE;
}

export function storeAppearanceMode(mode: AppearanceMode): void
{
  if (typeof window === "undefined")
  {
    return;
  }

  try
  {
    window.localStorage.setItem(APPEARANCE_STORAGE_KEY, mode);
  }
  catch
  {
    return;
  }
}

export function applyAppearanceMode(mode: AppearanceMode): void
{
  if (typeof document === "undefined")
  {
    return;
  }

  const root = document.documentElement;

  root.classList.toggle("dark", mode === "dark");
  root.dataset.appearanceMode = mode;
}

export function subscribeToAppearance(listener: AppearanceListener): () => void
{
  appearanceListeners.add(listener);

  return () =>
  {
    appearanceListeners.delete(listener);
  };
}

export function setAppearanceMode(mode: AppearanceMode): void
{
  applyAppearanceMode(mode);
  storeAppearanceMode(mode);

  appearanceListeners.forEach((listener) =>
  {
    listener();
  });
}
