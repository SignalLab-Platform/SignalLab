import { useAuth } from "@clerk/nextjs";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { act, renderHook, waitFor } from "@testing-library/react";

import { createOrganization } from "@/features/organizations/create-organization";
import { useCreateOrganization } from "@/features/organizations/use-create-organization";
import { organizationsQueryKey } from "@/features/organizations/use-organizations";

const mockGetToken = jest.fn();

jest.mock("@clerk/nextjs", () =>
({
  useAuth: jest.fn(),
}));

jest.mock("@/features/organizations/create-organization");

function createWrapper(queryClient: QueryClient)
{
  return function Wrapper(
    { children }: Readonly<{ children: React.ReactNode }>
  )
  {
    return (
      <QueryClientProvider client={queryClient}>
        {children}
      </QueryClientProvider>
    );
  };
}

function createQueryClient()
{
  return new QueryClient(
  {
    defaultOptions:
    {
      queries:
      {
        retry: false,
      },
      mutations:
      {
        retry: false,
      },
    },
  });
}

describe("useCreateOrganization", () =>
{
  beforeEach(() =>
  {
    jest.clearAllMocks();

    jest.mocked(useAuth).mockReturnValue(
    {
      getToken: mockGetToken,
      isLoaded: true,
      isSignedIn: true,
    } as unknown as ReturnType<typeof useAuth>);
  });

  it("creates an organization using the Clerk token", async () =>
  {
    mockGetToken.mockResolvedValue("test-token");

    const input =
    {
      name: "Example Studio",
      country: "France",
      website: null,
    };

    const organization =
    {
      id: "7e34f695-46df-4c29-9852-030783798531",
      ...input,
    };

    jest.mocked(createOrganization).mockResolvedValue(organization);

    const queryClient = createQueryClient();

    const { result } = renderHook(
      () => useCreateOrganization(),
      {
        wrapper: createWrapper(queryClient),
      }
    );

    let createdOrganization;

    await act(async () =>
    {
      createdOrganization = await result.current.mutateAsync(input);
    });

    expect(mockGetToken).toHaveBeenCalledTimes(1);

    expect(createOrganization).toHaveBeenCalledWith(
    {
      token: "test-token",
      input,
    });

    expect(createdOrganization).toEqual(organization);

    await waitFor(() =>
    {
      expect(result.current.isSuccess).toBe(true);
    });
  });

  it("invalidates the organizations query after successful creation", async () =>
  {
    mockGetToken.mockResolvedValue("test-token");

    const input =
    {
      name: "Example Studio",
      country: "France",
      website: null,
    };

    jest.mocked(createOrganization).mockResolvedValue(
    {
      id: "organization-1",
      ...input,
    });

    const queryClient = createQueryClient();
    const invalidateQueries = jest.spyOn(queryClient, "invalidateQueries");

    const { result } = renderHook(
      () => useCreateOrganization(),
      {
        wrapper: createWrapper(queryClient),
      }
    );

    await act(async () =>
    {
      await result.current.mutateAsync(input);
    });

    expect(invalidateQueries).toHaveBeenCalledWith(
    {
      queryKey: organizationsQueryKey,
    });
  });

  it("fails when Clerk cannot provide an authentication token", async () =>
  {
    mockGetToken.mockResolvedValue(null);

    const queryClient = createQueryClient();

    const { result } = renderHook(
      () => useCreateOrganization(),
      {
        wrapper: createWrapper(queryClient),
      }
    );

    const input =
    {
      name: "Example Studio",
      country: "France",
      website: null,
    };

    await act(async () =>
    {
      await expect(
        result.current.mutateAsync(input)
      ).rejects.toThrow(
        "Unable to obtain an authentication token for the current user."
      );
    });

    expect(createOrganization).not.toHaveBeenCalled();
    expect(result.current.isError).toBe(true);
  });

  it("propagates API errors without invalidating the organizations query", async () =>
  {
    mockGetToken.mockResolvedValue("test-token");

    jest.mocked(createOrganization).mockRejectedValue(
      new Error("SignalLab API request failed with status 400.")
    );

    const queryClient = createQueryClient();
    const invalidateQueries = jest.spyOn(queryClient, "invalidateQueries");

    const { result } = renderHook(
      () => useCreateOrganization(),
      {
        wrapper: createWrapper(queryClient),
      }
    );

    await act(async () =>
    {
      await expect(
        result.current.mutateAsync(
        {
          name: "",
          country: "France",
          website: null,
        })
      ).rejects.toThrow(
        "SignalLab API request failed with status 400."
      );
    });

    expect(result.current.isError).toBe(true);
    expect(invalidateQueries).not.toHaveBeenCalled();
  });
});