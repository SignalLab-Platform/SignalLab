import { redirect } from "next/navigation";

import { HOME_ROUTE } from "@/navigation/navigation-routes";

export default function RootPage()
{
  redirect(HOME_ROUTE);
}
