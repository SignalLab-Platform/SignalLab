import
{
  fireEvent,
  render,
  screen,
  waitFor,
} from "@testing-library/react";

import ProfilePage from "@/app/(application)/profile/page";

import
{
  useChangeCurrentUserAvatar,
} from "@/features/current-user-profile/use-change-current-user-avatar";
import
{
  useChangeCurrentUserEmail,
} from "@/features/current-user-profile/use-change-current-user-email";
import
{
  useCurrentUserEmailReconciliation,
} from "@/features/current-user-profile/use-current-user-email-reconciliation";
import
{
  useChangeCurrentUserFirstName,
} from "@/features/current-user-profile/use-change-current-user-first-name";
import
{
  useChangeCurrentUserLastName,
} from "@/features/current-user-profile/use-change-current-user-last-name";
import
{
  useCurrentUserProfile,
} from "@/features/current-user-profile/use-current-user-profile";

jest.mock(
  "@/features/current-user-profile/use-change-current-user-avatar",
);

jest.mock(
  "@/features/current-user-profile/use-change-current-user-email",
);

jest.mock(
  "@/features/current-user-profile/use-current-user-email-reconciliation",
);

jest.mock(
  "@/features/current-user-profile/use-change-current-user-first-name",
);

jest.mock(
  "@/features/current-user-profile/use-change-current-user-last-name",
);

jest.mock(
  "@/features/current-user-profile/use-current-user-profile",
);

const mockChangeAvatar = jest.fn();
const mockChangeFirstName = jest.fn();
const mockChangeLastName = jest.fn();
const mockRequestEmailChange = jest.fn();
const mockVerifyEmailChange = jest.fn();
const mockRetryEmailPromotion = jest.fn();
const mockRetryEmailSynchronization = jest.fn();
const mockResetEmailChange = jest.fn();

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

function mockLoadedProfile()
{
  jest.mocked(useCurrentUserProfile).mockReturnValue(
  {
    data: profile,
    isPending: false,
    isError: false,
  } as ReturnType<typeof useCurrentUserProfile>);
}

describe("ProfilePage", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();

    jest.mocked(useChangeCurrentUserAvatar).mockReturnValue(
    {
      mutateAsync: mockChangeAvatar,
      isPending: false,
      isError: false,
    } as unknown as ReturnType<typeof useChangeCurrentUserAvatar>);

    jest.mocked(useChangeCurrentUserEmail).mockReturnValue(
    {
      step: "idle",
      isPending: false,
      error: null,
      pendingEmail: null,
      requestEmailChange: mockRequestEmailChange,
      verifyEmailChange: mockVerifyEmailChange,
      retryPromotion: mockRetryEmailPromotion,
      retrySynchronization: mockRetryEmailSynchronization,
      reset: mockResetEmailChange,
    });

    jest.mocked(useCurrentUserEmailReconciliation).mockReturnValue(
    {
      needsReconciliation: jest.fn().mockReturnValue(false),
      reconcile: jest.fn(),
      isPending: false,
      isError: false,
      error: null,
      reset: jest.fn(),
    });

    jest.mocked(useChangeCurrentUserFirstName).mockReturnValue(
    {
      mutateAsync: mockChangeFirstName,
      isPending: false,
      isError: false,
    } as unknown as ReturnType<typeof useChangeCurrentUserFirstName>);

    jest.mocked(useChangeCurrentUserLastName).mockReturnValue(
    {
      mutateAsync: mockChangeLastName,
      isPending: false,
      isError: false,
    } as unknown as ReturnType<typeof useChangeCurrentUserLastName>);
  });

  it("renders the current SignalLab user profile", () =>
  {
    mockLoadedProfile();

    render(<ProfilePage />);

    expect(
      screen.getByRole("heading",
      {
        name: "My Profile",
      })
    ).toBeInTheDocument();

    expect(
      screen.getByText("Jimmy Fromonot")
    ).toBeInTheDocument();

    expect(
      screen.getAllByText("user@example.com").length
    ).toBeGreaterThan(0);

    expect(
      screen.getByText("English")
    ).toBeInTheDocument();

    expect(
      screen.getByText("JF")
    ).toBeInTheDocument();

    expect(
      screen.getByRole("button",
      {
        name: "Change profile picture",
      })
    ).toBeInTheDocument();
  });

  it("renders the current avatar", () =>
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

    render(<ProfilePage />);

    const avatar = screen.getByRole("presentation");

    expect(avatar).toHaveAttribute(
      "src",
      "https://storage.example.test/avatar.png",
    );

    expect(
      screen.queryByText("JF")
    ).not.toBeInTheDocument();
  });

  it("renders unset optional profile information", () =>
  {
    jest.mocked(useCurrentUserProfile).mockReturnValue(
    {
      data:
      {
        ...profile,
        firstName: null,
        lastName: null,
      },
      isPending: false,
      isError: false,
    } as ReturnType<typeof useCurrentUserProfile>);

    render(<ProfilePage />);

    expect(
      screen.getByText("SignalLab user")
    ).toBeInTheDocument();

    expect(
      screen.getAllByText("Not set")
    ).toHaveLength(2);

    expect(
      screen.getByText("U")
    ).toBeInTheDocument();
  });

  it("renders the loading state", () =>
  {
    jest.mocked(useCurrentUserProfile).mockReturnValue(
    {
      data: undefined,
      isPending: true,
      isError: false,
    } as ReturnType<typeof useCurrentUserProfile>);

    render(<ProfilePage />);

    expect(
      screen.getByText("Loading profile...")
    ).toBeInTheDocument();
  });

  it("renders the error state", () =>
  {
    jest.mocked(useCurrentUserProfile).mockReturnValue(
    {
      data: undefined,
      isPending: false,
      isError: true,
    } as ReturnType<typeof useCurrentUserProfile>);

    render(<ProfilePage />);

    expect(
      screen.getByText("Unable to load your profile.")
    ).toBeInTheDocument();
  });

  it("changes the profile picture with the selected file", async () =>
  {
    mockLoadedProfile();
    mockChangeAvatar.mockResolvedValue(undefined);

    const { container } = render(<ProfilePage />);

    const fileInput = container.querySelector(
      'input[type="file"]',
    );

    expect(fileInput).not.toBeNull();

    const file = new File(
      ["avatar-content"],
      "avatar.png",
      {
        type: "image/png",
      },
    );

    fireEvent.change(
      fileInput!,
      {
        target:
        {
          files: [file],
        },
      },
    );

    await waitFor(() =>
    {
      expect(mockChangeAvatar).toHaveBeenCalledWith(file);
    });
  });

  it("renders the profile picture error", () =>
  {
    mockLoadedProfile();

    jest.mocked(useChangeCurrentUserAvatar).mockReturnValue(
    {
      mutateAsync: mockChangeAvatar,
      isPending: false,
      isError: true,
    } as unknown as ReturnType<typeof useChangeCurrentUserAvatar>);

    render(<ProfilePage />);

    expect(
      screen.getByText(
        "Unable to change your profile picture.",
      )
    ).toBeInTheDocument();
  });

  it("opens the personal information editor with the current values", () =>
  {
    mockLoadedProfile();

    render(<ProfilePage />);

    fireEvent.click(
      screen.getAllByRole("button",
      {
        name: "Edit",
      })[0]
    );

    expect(
      screen.getByRole("textbox",
      {
        name: "First name",
      })
    ).toHaveValue("Jimmy");

    expect(
      screen.getByRole("textbox",
      {
        name: "Last name",
      })
    ).toHaveValue("Fromonot");

    expect(
      screen.getAllByText("user@example.com").length
    ).toBeGreaterThan(0);

    expect(
      screen.getByText("English")
    ).toBeInTheDocument();

    expect(
      screen.getByRole("button",
      {
        name: "Save",
      })
    ).toBeInTheDocument();

    expect(
      screen.getByRole("button",
      {
        name: "Cancel",
      })
    ).toBeInTheDocument();
  });

  it("cancels personal information changes", () =>
  {
    mockLoadedProfile();

    render(<ProfilePage />);

    fireEvent.click(
      screen.getAllByRole("button",
      {
        name: "Edit",
      })[0]
    );

    fireEvent.change(
      screen.getByRole("textbox",
      {
        name: "First name",
      }),
      {
        target:
        {
          value: "Changed",
        },
      },
    );

    fireEvent.click(
      screen.getByRole("button",
      {
        name: "Cancel",
      })
    );

    expect(
      screen.queryByRole("textbox",
      {
        name: "First name",
      })
    ).not.toBeInTheDocument();

    expect(
      screen.getByText("Jimmy Fromonot")
    ).toBeInTheDocument();

    expect(mockChangeFirstName).not.toHaveBeenCalled();
    expect(mockChangeLastName).not.toHaveBeenCalled();
  });

  it("saves only changed personal information", async () =>
  {
    mockLoadedProfile();

    mockChangeFirstName.mockResolvedValue(
    {
      firstName: "James",
    });

    render(<ProfilePage />);

    fireEvent.click(
      screen.getAllByRole("button",
      {
        name: "Edit",
      })[0]
    );

    fireEvent.change(
      screen.getByRole("textbox",
      {
        name: "First name",
      }),
      {
        target:
        {
          value: "James",
        },
      },
    );

    fireEvent.click(
      screen.getByRole("button",
      {
        name: "Save",
      })
    );

    await waitFor(() =>
    {
      expect(mockChangeFirstName).toHaveBeenCalledWith("James");
    });

    expect(mockChangeLastName).not.toHaveBeenCalled();
  });

  it("clears an optional personal information field with null", async () =>
  {
    mockLoadedProfile();

    mockChangeLastName.mockResolvedValue(
    {
      lastName: null,
    });

    render(<ProfilePage />);

    fireEvent.click(
      screen.getAllByRole("button",
      {
        name: "Edit",
      })[0]
    );

    fireEvent.change(
      screen.getByRole("textbox",
      {
        name: "Last name",
      }),
      {
        target:
        {
          value: "   ",
        },
      },
    );

    fireEvent.click(
      screen.getByRole("button",
      {
        name: "Save",
      })
    );

    await waitFor(() =>
    {
      expect(mockChangeLastName).toHaveBeenCalledWith(null);
    });

    expect(mockChangeFirstName).not.toHaveBeenCalled();
  });
});