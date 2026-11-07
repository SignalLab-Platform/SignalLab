import { ClerkProvider } from "@clerk/nextjs";
import type { Metadata } from "next";
import { Geist } from "next/font/google";

import { AppProviders } from "@/providers/app-providers";

import "./globals.css";

import { APPEARANCE_INITIALIZER_SCRIPT } from "@/lib/appearance";
import { SIDEBAR_INITIALIZER_SCRIPT } from "@/lib/sidebar";

const geist = Geist(
{
  subsets: ["latin"],
  variable: "--font-geist-sans",
  display: "swap",
});

export const metadata: Metadata =
{
  title: "SignalLab",
  description: "Game User Research platform",
};

export default function RootLayout(
{
  children,
}: Readonly<
{
  children: React.ReactNode;
}>)
{
  return (
    <html
      lang="en"
      className={geist.variable}
      suppressHydrationWarning
    >
      <head>
        <script
          id="appearance-initializer"
          dangerouslySetInnerHTML={{ __html: APPEARANCE_INITIALIZER_SCRIPT }}
        />

        <script
          id="sidebar-initializer"
          dangerouslySetInnerHTML={{ __html: SIDEBAR_INITIALIZER_SCRIPT }}
        />
      </head>

      <body>
        <ClerkProvider>
          <AppProviders>
            {children}
          </AppProviders>
        </ClerkProvider>
      </body>
    </html>
  );
}
