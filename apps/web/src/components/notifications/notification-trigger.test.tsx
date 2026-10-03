import { render, screen } from "@testing-library/react";

import { NotificationTrigger } from "@/components/notifications/notification-trigger";

describe("NotificationTrigger", () =>
{
  it("reserves the global Notifications control", () =>
  {
    render(<NotificationTrigger />);

    expect(
      screen.getByRole("button",
      {
        name: "Notifications",
      })
    ).toBeInTheDocument();
  });
});