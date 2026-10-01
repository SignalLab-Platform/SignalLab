import { clerkMiddleware } from "@clerk/nextjs/server";
import { NextResponse } from "next/server";

export const proxy = clerkMiddleware((auth, request) =>
{
  const response = NextResponse.next();

  const hostname = request.headers
    .get("host")
    ?.split(":")[0]
    .toLowerCase();

  const isStaging =
    hostname === "staging.signallab.dev" ||
    hostname?.endsWith(".onrender.com");

  if (isStaging)
  {
    response.headers.set(
      "X-Robots-Tag",
      "noindex, nofollow",
    );
  }

  return response;
});

export const config =
{
  matcher: [
    "/((?!_next/static|_next/image|favicon.ico).*)",
  ],
};