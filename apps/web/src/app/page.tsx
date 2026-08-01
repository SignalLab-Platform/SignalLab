import { Button } from "@/components/ui/button";

export default function Home() {
  return (
    <main className="flex min-h-screen items-center justify-center p-8">
      <section className="space-y-4 text-center">
        <p className="text-sm font-medium text-muted-foreground">Platform Foundation</p>

        <h1 className="text-3xl font-semibold tracking-tight">SignalLab</h1>

        <p className="max-w-md text-muted-foreground">
          The frontend foundation is ready for the future application shell.
        </p>

        <Button disabled>Application shell not implemented</Button>
      </section>
    </main>
  );
}
