import { auth } from "@clerk/nextjs/server";

import { ApplicationViewport } from "@/components/shell/application-viewport";

type ApplicationLayoutProps =
{
  children: React.ReactNode;
};

export default async function ApplicationLayout({ children }: Readonly<ApplicationLayoutProps>)
{
  await auth.protect();

  return (
    <ApplicationViewport>
      {children}
    </ApplicationViewport>
  );
}