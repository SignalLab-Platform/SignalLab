# SignalLab Web

The SignalLab Frontend is a React application built with Next.js App Router.

It currently provides the technical foundation of the user interface without implementing the SignalLab Application Shell or any business domain.

## Technology stack

* Next.js App Router
* React
* TypeScript
* Tailwind CSS
* shadcn/ui with the Nova preset
* TanStack Query
* Jest
* React Testing Library

Routing is handled exclusively by Next.js.

Vite, TanStack Router, and additional global state libraries are not used.

## Requirements

* Node.js 24.18.0 or a compatible Node.js 24 version
* npm 11 or a compatible version

The reference Node.js version is defined in the repository root `.nvmrc`.

Verify the active versions:

```powershell
node --version
npm.cmd --version
```

On Windows PowerShell, `npm.cmd` and `npx.cmd` can be used when script execution policies prevent the `.ps1` wrappers from running.

## Installation

From the repository root:

```powershell
npm.cmd --prefix apps/web clean-install
```

Or from `apps/web`:

```powershell
npm.cmd clean-install
```

## Development

From `apps/web`:

```powershell
npm.cmd run dev
```

The application is available by default at:

```text
http://localhost:3000
```

## Build

```powershell
npm.cmd run build
```

## Lint

```powershell
npm.cmd run lint
```

## Type checking

```powershell
npm.cmd run typecheck
```

## Tests

Run the Jest test suite:

```powershell
npm.cmd test
```

Run tests interactively:

```powershell
npm.cmd run test:watch
```

## Source structure

```text
src/
├── app/
│   ├── globals.css
│   ├── layout.tsx
│   ├── page.test.tsx
│   └── page.tsx
├── components/
│   └── ui/
├── lib/
└── providers/
```

### `app`

Owns Next.js routes, layouts, route metadata, and global styles.

### `components/ui`

Contains locally generated shadcn/ui primitives.

These components remain part of the SignalLab source code and can be adapted when the visual system is formalized.

### `providers`

Contains root-level React providers.

TanStack Query is initialized here through a single `QueryClientProvider`.

### `lib`

Contains shared technical utilities that do not belong to a business domain.

## Server State

TanStack Query is initialized at the root of the application.

No artificial query or API client is currently implemented because the Backend does not yet expose a business endpoint.

Future remote data must use TanStack Query when it represents Server State.

## Routing

The application uses Next.js App Router exclusively.

Routes are represented by the `src/app` directory.

The project does not use:

* Vite;
* TanStack Router;
* React Router.

## Typography

Geist is the functional typeface used for interface text and everyday content.

Faune by Alice Savoie is reserved for the future SignalLab visual identity and has not yet been bundled into the application.

Its targeted use and licensing attribution will be introduced with the visual system rather than during this foundation Spec.

## Component system

shadcn/ui is configured with:

* the Nova style;
* Radix-based primitives;
* CSS variables;
* Tailwind CSS;
* Lucide icons.

The current Button component validates the installation without defining the final SignalLab visual identity.

## Dependency security

Audit direct and transitive dependencies:

```powershell
npm.cmd audit
```

PostCSS and Sharp are currently pinned through npm overrides because the versions originally resolved through Next.js were affected by published security advisories.

Installation scripts are explicitly authorized only for the reviewed Sharp and `unrs-resolver` versions.

These overrides must be reviewed when Next.js is upgraded.

## Current scope

The Frontend currently contains no:

* Application Shell;
* Header or Sidebar;
* business route;
* API client;
* authentication;
* persisted application state;
* Organization or Project context;
* mobile interface;
* Faune font files;
* business logic.

These capabilities are introduced only by their dedicated specifications.
