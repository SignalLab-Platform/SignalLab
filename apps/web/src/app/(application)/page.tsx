const shellRegions =
[
  {
    name: "Header",
    description: "Global identity and application-wide controls remain visible across routes.",
  },
  {
    name: "Sidebar",
    description: "Context navigation will remain independent from the current content.",
  },
  {
    name: "Content",
    description: "Each route renders its own page inside this scrollable region.",
  },
];

export default function Home()
{
  return (
    <section className="mx-auto flex min-h-full w-full max-w-5xl flex-col justify-center gap-8 px-8 py-12">
      <div className="max-w-2xl space-y-4">
        <p className="text-sm font-medium text-primary">
          Platform Foundation
        </p>

        <h1 className="text-4xl font-semibold tracking-tight">
          Application shell
        </h1>

        <p className="text-base leading-7 text-muted-foreground">
          Header, Sidebar, and Content are now hosted by a persistent application layout.
        </p>
      </div>

      <div className="grid grid-cols-3 gap-4">
        {shellRegions.map((region) =>
        {
          return (
            <article
              key={region.name}
              className="rounded-xl border bg-card p-5 text-card-foreground shadow-sm"
            >
              <div className="mb-4 size-2 rounded-full bg-primary" />

              <h2 className="font-semibold">
                {region.name}
              </h2>

              <p className="mt-2 text-sm leading-6 text-muted-foreground">
                {region.description}
              </p>
            </article>
          );
        })}
      </div>
    </section>
  );
}
