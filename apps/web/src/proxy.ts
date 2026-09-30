import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

export function proxy(request: NextRequest)
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
}

export const config = {
  matcher: [
    "/((?!_next/static|_next/image|favicon.ico).*)",
  ],
};