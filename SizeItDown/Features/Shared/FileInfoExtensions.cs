namespace SizeItDown.Generators;

public static class FileInfoExtensions
{
    public static FileInfo Rename(this FileInfo fileInfo, string newName)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);

        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("New name must be a valid, non-empty string.", nameof(newName));

        string directory = fileInfo.DirectoryName!;
        string newFilePath = Path.Combine(directory, newName);

        File.Move(fileInfo.FullName, newFilePath);

        return new FileInfo(newFilePath);
    }

    /// <summary>
    /// Gets the file name without its extension.
    /// </summary>
    /// <param name="fileInfo">The FileInfo object.</param>
    /// <returns>File name without extension.</returns>
    public static string NameWithoutExtension(this FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);

        return Path.GetFileNameWithoutExtension(fileInfo.Name);
    }
    
    public static FileInfo AddPrefix(this FileInfo fileInfo, string prefix)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        ArgumentNullException.ThrowIfNull(prefix);

        string nameWithoutExt = Path.GetFileNameWithoutExtension(fileInfo.Name);
        string extension = fileInfo.Extension;
        string newName = prefix + nameWithoutExt + extension;

        return fileInfo.Rename(newName);
    }

    public static FileInfo AddPostfix(this FileInfo fileInfo, string postfix)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);
        ArgumentNullException.ThrowIfNull(postfix);

        string nameWithoutExt = Path.GetFileNameWithoutExtension(fileInfo.Name);
        string extension = fileInfo.Extension;
        string newName = nameWithoutExt + postfix + extension;

        return fileInfo.Rename(newName);
    }
}