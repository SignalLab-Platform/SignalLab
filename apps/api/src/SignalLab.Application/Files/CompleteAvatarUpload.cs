using SignalLab.Application.Storage;
using SignalLab.Application.Users;
using SignalLab.Domain.Files;
using SignalLab.Domain.Users;
using SignalLab.Application.Errors;

namespace SignalLab.Application.Files;

public sealed class CompleteAvatarUpload
{
    private readonly IStoredFileRepository storedFileRepository;
    private readonly IUserRepository userRepository;
    private readonly IObjectStorage objectStorage;

    public CompleteAvatarUpload(
        IStoredFileRepository storedFileRepository,
        IUserRepository userRepository,
        IObjectStorage objectStorage)
    {
        this.storedFileRepository = storedFileRepository;
        this.userRepository = userRepository;
        this.objectStorage = objectStorage;
    }

    public async Task ExecuteAsync(
        UserId userId,
        StoredFileId storedFileId,
        CancellationToken cancellationToken)
    {
        var storedFile = await storedFileRepository.GetByIdAsync(
            storedFileId,
            cancellationToken);

        if (storedFile is null)
        {
            throw new ResourceNotFoundException(
                FileErrorCodes.StoredFileNotFound,
                "Stored file could not be found.");
        }

        if (storedFile.OwnerUserId != userId)
        {
            throw new ResourceNotFoundException(
                FileErrorCodes.StoredFileNotFound,
                "Stored file could not be found.");
        }

        if (storedFile.Status != StoredFileStatus.Pending)
        {
            throw new ConflictException(
                FileErrorCodes.UploadNotPending,
                "Stored file is not pending.");
        }

        var metadata = await objectStorage.GetMetadataAsync(
            storedFile.ObjectKey,
            cancellationToken);

        if (metadata is null)
        {
            throw new ConflictException(
                FileErrorCodes.UploadedObjectNotFound,
                "Uploaded object could not be found.");
        }

        if (!string.Equals(
            metadata.ContentType,
            storedFile.ContentType,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException(
                FileErrorCodes.UploadedObjectContentTypeMismatch,
                "Uploaded object content type does not match the expected content type.");
        }

        if (metadata.Size != storedFile.Size)
        {
            throw new ConflictException(
                FileErrorCodes.UploadedObjectSizeMismatch,
                "Uploaded object size does not match the expected size.");
        }

        var user = await userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user is null)
        {
            throw new ResourceNotFoundException(
                UserErrorCodes.UserNotFound,
                "SignalLab user could not be found.");
        }

        StoredFile? previousAvatar = null;

        if (user.AvatarFileId is not null)
        {
            previousAvatar = await storedFileRepository.GetByIdAsync(
                user.AvatarFileId.Value,
                cancellationToken);
        }

        storedFile.MarkAvailable();
        user.ChangeAvatar(storedFile.Id);

        if (previousAvatar is not null)
        {
            storedFileRepository.Remove(previousAvatar);
        }

        await userRepository.SaveChangesAsync(
            cancellationToken);

        if (previousAvatar is not null)
        {
            await objectStorage.DeleteAsync(
                previousAvatar.ObjectKey,
                cancellationToken);
        }
    }
}