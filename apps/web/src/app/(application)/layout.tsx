import { ApplicationViewport } from "@/components/shell/application-viewport";

type ApplicationLayoutProps =
{
  children: React.ReactNode;
};

export default function ApplicationLayout({ children }: Readonly<ApplicationLayoutProps>)
{
  return (
    <ApplicationViewport>
      {children}
    </ApplicationViewport>
  );
}
