using Microsoft.EntityFrameworkCore;
using SignalLab.Application.Files;
using SignalLab.Domain.Files;

namespace SignalLab.Infrastructure.Persistence.Repositories;

public sealed class StoredFileRepository : IStoredFileRepository
{
    private readonly SignalLabDbContext dbContext;

    public StoredFileRepository(SignalLabDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<StoredFile?> GetByIdAsync(
        StoredFileId storedFileId,
        CancellationToken cancellationToken)
    {
        return dbContext.StoredFiles
            .SingleOrDefaultAsync(
                storedFile => storedFile.Id == storedFileId,
                cancellationToken);
    }

    public void Add(StoredFile storedFile)
    {
        dbContext.StoredFiles.Add(storedFile);
    }

    public void Remove(StoredFile storedFile)
    {
        dbContext.StoredFiles.Remove(storedFile);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}