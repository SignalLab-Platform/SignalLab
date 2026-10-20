import
{
  render,
  screen,
} from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import { UserMenu } from "@/components/identity/user-menu";

import
{
  useCurrentUserProfile,
} from "@/features/current-user-profile/use-current-user-profile";

const signOut = jest.fn();

jest.mock("@clerk/nextjs", () =>
{
  return {
    useClerk: () =>
    {
      return {
        signOut,
      };
    },
  };
});

jest.mock(
  "@/features/current-user-profile/use-current-user-profile",
);

const profile =
{
  id: "7e34f695-46df-4c29-9852-030783798531",
  email: "user@example.com",
  firstName: "Jimmy",
  lastName: "Fromonot",
  language: "English",
  avatarFileId: null,
  avatarUrl: null,
};

describe("UserMenu", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();

    jest.mocked(useCurrentUserProfile).mockReturnValue(
    {
      data: profile,
      isPending: false,
      isError: false,
    } as ReturnType<typeof useCurrentUserProfile>);
  });

  it("renders the fallback avatar without a profile picture", () =>
  {
    render(<UserMenu />);

    expect(
      screen.getByText("U")
    ).toBeInTheDocument();
  });

  it("renders the current profile picture", () =>
  {
    jest.mocked(useCurrentUserProfile).mockReturnValue(
    {
      data:
      {
        ...profile,
        avatarFileId: "a6a4628e-413b-44c1-b1bd-6ef854782bfd",
        avatarUrl: "https://storage.example.test/avatar.png",
      },
      isPending: false,
      isError: false,
    } as ReturnType<typeof useCurrentUserProfile>);

    render(<UserMenu />);

    const avatar = screen.getByRole("presentation");

    expect(avatar).toHaveAttribute(
      "src",
      "https://storage.example.test/avatar.png",
    );

    expect(
      screen.queryByText("U")
    ).not.toBeInTheDocument();
  });

  it("opens the user menu", async () =>
  {
    const user = userEvent.setup();

    render(<UserMenu />);

    await user.click(
      screen.getByRole("button",
      {
        name: "User menu",
      })
    );

    expect(
      screen.getByRole("menu")
    ).toBeInTheDocument();
  });

  it("provides access to the SignalLab profile and settings", async () =>
  {
    const user = userEvent.setup();

    render(<UserMenu />);

    await user.click(
      screen.getByRole("button",
      {
        name: "User menu",
      })
    );

    expect(
      screen.getByRole("menuitem",
      {
        name: "My Profile",
      })
    ).toHaveAttribute("href", "/profile");

    expect(
      screen.getByRole("menuitem",
      {
        name: "Settings",
      })
    ).toHaveAttribute("href", "/settings");
  });

  it("signs out through Clerk", async () =>
  {
    const user = userEvent.setup();

    render(<UserMenu />);

    await user.click(
      screen.getByRole("button",
      {
        name: "User menu",
      })
    );

    await user.click(
      screen.getByRole("menuitem",
      {
        name: "Sign out",
      })
    );

    expect(signOut).toHaveBeenCalledTimes(1);
  });
});