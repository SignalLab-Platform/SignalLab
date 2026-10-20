import
{
  fireEvent,
  render,
  screen,
  waitFor,
} from "@testing-library/react";

import
{
  ProfileEmailEditor,
} from "@/features/current-user-profile/profile-email-editor";
import
{
  useChangeCurrentUserEmail,
} from "@/features/current-user-profile/use-change-current-user-email";
import
{
  useCurrentUserEmailReconciliation,
} from "@/features/current-user-profile/use-current-user-email-reconciliation";

jest.mock(
  "@/features/current-user-profile/use-change-current-user-email",
);

jest.mock(
  "@/features/current-user-profile/use-current-user-email-reconciliation",
);

describe("ProfileEmailEditor", () =>
{
  const requestEmailChange = jest.fn();
  const verifyEmailChange = jest.fn();
  const retryPromotion = jest.fn();
  const retrySynchronization = jest.fn();
  const resetEmailChange = jest.fn();

  const needsReconciliation = jest.fn();
  const reconcile = jest.fn();
  const resetReconciliation = jest.fn();

  function mockEmailChange(
    overrides: Partial<ReturnType<typeof useChangeCurrentUserEmail>> = {},
  )
  {
    jest.mocked(useChangeCurrentUserEmail).mockReturnValue(
    {
      step: "idle",
      isPending: false,
      error: null,
      pendingEmail: null,
      requestEmailChange,
      verifyEmailChange,
      retryPromotion,
      retrySynchronization,
      reset: resetEmailChange,
      ...overrides,
    });
  }

  function mockReconciliation(
    overrides: Partial<ReturnType<typeof useCurrentUserEmailReconciliation>> = {},
  )
  {
    jest.mocked(useCurrentUserEmailReconciliation).mockReturnValue(
    {
      needsReconciliation,
      reconcile,
      isPending: false,
      isError: false,
      error: null,
      reset: resetReconciliation,
      ...overrides,
    });
  }

  beforeEach(() =>
  {
    jest.resetAllMocks();

    requestEmailChange.mockResolvedValue(undefined);
    verifyEmailChange.mockResolvedValue(undefined);
    retryPromotion.mockResolvedValue(undefined);
    retrySynchronization.mockResolvedValue(undefined);
    reconcile.mockResolvedValue(undefined);

    needsReconciliation.mockReturnValue(false);

    mockEmailChange();
    mockReconciliation();
  });

  it("displays the current email and an Edit button", () =>
  {
    render(<ProfileEmailEditor email="old@example.com" />);

    expect(screen.getByText("old@example.com")).toBeInTheDocument();

    expect(
      screen.getByRole("button", { name: "Edit" }),
    ).toBeInTheDocument();
  });

  it("requests a change to the entered email address", async () =>
  {
    render(<ProfileEmailEditor email="old@example.com" />);

    fireEvent.click(
      screen.getByRole("button", { name: "Edit" }),
    );

    fireEvent.change(
      screen.getByLabelText("Email"),
      {
        target: { value: "  new@example.com  " },
      },
    );

    fireEvent.click(
      screen.getByRole("button", { name: "Save" }),
    );

    await waitFor(() =>
    {
      expect(requestEmailChange).toHaveBeenCalledWith(
        "new@example.com",
      );
    });
  });

  it("does not allow saving an unchanged email", () =>
  {
    render(<ProfileEmailEditor email="old@example.com" />);

    fireEvent.click(
      screen.getByRole("button", { name: "Edit" }),
    );

    expect(
      screen.getByRole("button", { name: "Save" }),
    ).toBeDisabled();

    expect(requestEmailChange).not.toHaveBeenCalled();
  });

  it("verifies the entered OTP code", async () =>
  {
    mockEmailChange(
    {
      step: "verification",
      pendingEmail: "new@example.com",
    });

    render(<ProfileEmailEditor email="old@example.com" />);

    expect(
      screen.getByText("new@example.com"),
    ).toBeInTheDocument();

    fireEvent.change(
      screen.getByLabelText("Verification code"),
      {
        target: { value: " 123456 " },
      },
    );

    fireEvent.click(
      screen.getByRole("button", { name: "Verify" }),
    );

    await waitFor(() =>
    {
      expect(verifyEmailChange).toHaveBeenCalledWith(
        " 123456 ",
      );
    });
  });

  it("cancels the email editing process", () =>
  {
    render(<ProfileEmailEditor email="old@example.com" />);

    fireEvent.click(
      screen.getByRole("button", { name: "Edit" }),
    );

    fireEvent.click(
      screen.getByRole("button", { name: "Cancel" }),
    );

    expect(resetEmailChange).toHaveBeenCalled();

    expect(
      screen.getByRole("button", { name: "Edit" }),
    ).toBeInTheDocument();
  });

  it("allows retrying a failed Clerk primary email promotion", async () =>
  {
    mockEmailChange(
    {
      step: "promotion_failed",
      error: "Clerk update failed.",
    });

    render(<ProfileEmailEditor email="old@example.com" />);

    expect(
      screen.getByText("Clerk update failed."),
    ).toBeInTheDocument();

    fireEvent.click(
      screen.getByRole("button", { name: "Retry email update" }),
    );

    await waitFor(() =>
    {
      expect(retryPromotion).toHaveBeenCalledTimes(1);
    });
  });

  it("allows retrying a failed SignalLab synchronization", async () =>
  {
    mockEmailChange(
    {
      step: "synchronization_failed",
      error: "Synchronization failed.",
    });

    render(<ProfileEmailEditor email="old@example.com" />);

    fireEvent.click(
      screen.getByRole("button", { name: "Retry synchronization" }),
    );

    await waitFor(() =>
    {
      expect(retrySynchronization).toHaveBeenCalledTimes(1);
    });
  });

  it("offers reconciliation when Clerk and SignalLab emails differ", async () =>
  {
    needsReconciliation.mockReturnValue(true);

    render(<ProfileEmailEditor email="old@example.com" />);

    expect(
      screen.queryByRole("button", { name: "Edit" }),
    ).not.toBeInTheDocument();

    fireEvent.click(
      screen.getByRole("button", { name: "Retry synchronization" }),
    );

    await waitFor(() =>
    {
      expect(reconcile).toHaveBeenCalledTimes(1);
    });
  });

  it("shows reconciliation errors", () =>
  {
    needsReconciliation.mockReturnValue(true);

    mockReconciliation(
    {
      isError: true,
      error: new Error("Backend unavailable."),
    });

    render(<ProfileEmailEditor email="old@example.com" />);

    expect(
      screen.getByRole("alert"),
    ).toHaveTextContent("Backend unavailable.");
  });

  it("displays the successful completion state", () =>
  {
    mockEmailChange(
    {
      step: "completed",
    });

    render(<ProfileEmailEditor email="new@example.com" />);

    expect(
      screen.getByText(
        "Your email address has been updated successfully.",
      ),
    ).toBeInTheDocument();

    fireEvent.click(
      screen.getByRole("button", { name: "Done" }),
    );

    expect(resetEmailChange).toHaveBeenCalled();
  });
});
