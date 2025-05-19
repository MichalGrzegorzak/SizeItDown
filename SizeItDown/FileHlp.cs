using System.Reflection;

namespace SizeItDown.Helpers;

public class FileHlp
{
    public static void SaveFile(string dir, StringBuilder sb, string toFileName)
    {
        var fullPath = @$"{dir}\{toFileName}";
        var text = sb.ToString();
        var linesCount = text.Count(c => c == '\n');
        
        File.WriteAllText(fullPath, text);
        Console.WriteLine($"Wrote lines: {linesCount}");
    }
    
    public static void EnsureDirStructure(string filePath)
    {
        string directoryPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            // Create the directory (and any missing parent directories)
            Directory.CreateDirectory(directoryPath);
        }
    }
    
    public static void SaveResourceFileOnDisk(string fileName)
    {
        var resourceName = $"SizeItDown.Resources.{fileName}";
        var outputPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
        if (Path.Exists(outputPath))
            return;

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new ApplicationException($"Resource not found: {fileName}");
        
        using var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        stream.CopyTo(fileStream);

        Console.WriteLine($"File saved to {outputPath}");
    }
}