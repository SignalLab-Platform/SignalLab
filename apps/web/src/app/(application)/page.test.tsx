import { redirect } from "next/navigation";

import RootPage from "./page";
import { HOME_ROUTE } from "@/navigation/navigation-routes";

jest.mock("next/navigation", () =>
{
  return {
    redirect: jest.fn(),
  };
});

describe("RootPage", () =>
{
  const mockedRedirect = jest.mocked(redirect);

  beforeEach(() =>
  {
    mockedRedirect.mockClear();
  });

  it("redirects the application root to Personal Home", () =>
  {
    RootPage();

    expect(mockedRedirect).toHaveBeenCalledWith(HOME_ROUTE);
  });
});
