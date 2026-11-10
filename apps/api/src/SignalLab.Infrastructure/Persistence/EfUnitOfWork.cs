using Microsoft.EntityFrameworkCore;
using Npgsql;
using SignalLab.Application.Organizations;
using SignalLab.Application.Persistence;

namespace SignalLab.Infrastructure.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private const string PendingInvitationIndex =
        "ux_organization_invitations_pending";

    private readonly SignalLabDbContext dbContext;

    public EfUnitOfWork(SignalLabDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (IsPendingInvitationConflict(exception))
        {
            throw new OrganizationInvitationAlreadyPendingException(
                exception);
        }
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await dbContext.Database.BeginTransactionAsync(
                    cancellationToken);

            await operation(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        });
    }

    private static bool IsPendingInvitationConflict(
        DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgresException &&
            postgresException.SqlState ==
                PostgresErrorCodes.UniqueViolation &&
            postgresException.ConstraintName == PendingInvitationIndex;
    }
}