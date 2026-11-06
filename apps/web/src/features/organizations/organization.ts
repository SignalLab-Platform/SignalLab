export type Organization = Readonly<
{
  id: string;
  name: string;
  country: string;
  website: string | null;
}>;