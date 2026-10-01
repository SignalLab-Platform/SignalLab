import { ClerkProvider } from "@clerk/nextjs";
import type { Metadata } from "next";
import { Geist } from "next/font/google";
import Script from "next/script";

import { APPEARANCE_INITIALIZER_SCRIPT } from "@/lib/appearance";
import { SIDEBAR_INITIALIZER_SCRIPT } from "@/lib/sidebar";
import { AppProviders } from "@/providers/app-providers";

import "./globals.css";

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
      <body>
        <Script
          id="appearance-initializer"
          strategy="beforeInteractive"
        >
          {APPEARANCE_INITIALIZER_SCRIPT}
        </Script>

        <Script
          id="sidebar-initializer"
          strategy="beforeInteractive"
        >
          {SIDEBAR_INITIALIZER_SCRIPT}
        </Script>

        <ClerkProvider>
          <AppProviders>
            {children}
          </AppProviders>
        </ClerkProvider>
      </body>
    </html>
  );
}
