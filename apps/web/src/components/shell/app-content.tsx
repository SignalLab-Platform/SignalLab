type AppContentProps =
{
  children: React.ReactNode;
};

export function AppContent({ children }: Readonly<AppContentProps>)
{
  return (
    <main
      id="application-content"
      className="min-h-0 min-w-0 overflow-y-auto bg-background text-foreground"
    >
      {children}
    </main>
  );
}
