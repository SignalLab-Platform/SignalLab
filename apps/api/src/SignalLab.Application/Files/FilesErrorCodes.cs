namespace SignalLab.Application.Files;

public static class FileErrorCodes
{
    public const string InvalidAvatarFileName = "invalid_avatar_file_name";
    public const string InvalidAvatarSize = "invalid_avatar_size";
    public const string UnsupportedAvatarContentType = "unsupported_avatar_content_type";

    public const string StoredFileNotFound = "stored_file_not_found";
    public const string UploadNotPending = "upload_not_pending";
    public const string UploadedObjectNotFound = "uploaded_object_not_found";
    public const string UploadedObjectContentTypeMismatch = "uploaded_object_content_type_mismatch";
    public const string UploadedObjectSizeMismatch = "uploaded_object_size_mismatch";
}