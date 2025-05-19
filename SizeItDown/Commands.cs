using System.Diagnostics;

namespace SizeItDown.Generators;

public static class Commands
{
    public static long CleanMotionFiles(Options o, List<string> mediaFiles)
    {
        long deletedFileSize = 0;
        var allMotionFiles = mediaFiles.Where(x => x.Contains(".MP")).ToList();
        foreach (var mpFilePath in allMotionFiles)
        {
            var motionJpgFilePath = mpFilePath + ".jpg";
            if (Path.Exists(motionJpgFilePath))
            {
                var newFileName = Path.ChangeExtension(mpFilePath, ".mp4");
                File.Move(mpFilePath, newFileName);
                mediaFiles.Add(newFileName);

                var jpgInfo = new FileInfo(motionJpgFilePath);
                deletedFileSize += jpgInfo.Length;
                jpgInfo.Delete();
            }
            else
            {
                Console.WriteLine($"Lonely Motion file detected: {motionJpgFilePath}");
            }
        }

        return deletedFileSize;
    }
    
    public static void Replace(Options o, List<string> inputVideoFiles, MyStringBuilder sb)
    {
        var outputVideoFiles = Const.VideoExtensions.SelectMany(ext => 
            Directory.EnumerateFiles(o.TempOutDir, $"*{ext}", SearchOption.AllDirectories)).ToList();
        
        var matchingNames = inputVideoFiles.Select(Path.GetFileName)
            .Intersect(outputVideoFiles.Select(Path.GetFileName))
            .ToList();

        sb.AppendLineAndConsole($"\nREPLACE Found matching files: {matchingNames.Count}");

        var sizes = new List<FileSizes>();
    
        foreach (string inputFilePath in inputVideoFiles)
        {
            if (matchingNames.FirstOrDefault(x => x == Path.GetFileName(inputFilePath)) == null)
            {
                continue; //skip fast
            }
            
            var shortInpFilePath = inputFilePath.Replace(o.InputDir, "");
            var outputFilePath = outputVideoFiles.First(path => path.Contains(shortInpFilePath));

            if (string.IsNullOrEmpty(outputFilePath))
            {
                sb.AppendLineAndConsole($"Can't find file: {outputFilePath}");
                Debugger.Break();
                continue;
            }
            
            var size = new FileSizes(new FileInfo(inputFilePath), new FileInfo(outputFilePath));
            sizes.Add(size);
            
            if (size.Diff >= 0)
            {
                if (!o.TestMode)
                {
                    File.Move(outputFilePath, inputFilePath, true);
                }
                var percent = size.Input.PercentChange(size.Output)*-1;
                sb.AppendLineAndConsole($"Replaced file: {shortInpFilePath}, sizes: {size.Input.ToKBStr()} -> {size.Output.ToKBStr()}, red: {size.Diff.ToKBStr()} ({percent}%)");
                sb.AppendLine(size.ToString());
            }
            else
            {
                sizes.Last().ResetDiff();
                sb.AppendLineAndConsole($"Size larger after compression, skipping file: {outputFilePath}, diff: {size.Diff} ");
                //if (!o.TestMode)
                {
                    File.Delete(outputFilePath);
                }
            }
        }
        
        var text = sb.ToString();
        sb.Clear();
        var lines = text.Replace("\r","").Split('\n');
        var sortedLines = lines.OrderBy(x => x).ToList();

        foreach (var line in sortedLines)
            sb.AppendLineAndConsole(line);

        sb.AppendLineAndConsole("");
        sb.AppendLineAndConsole("=====================");
        sb.AppendLineAndConsole($"Total reduction: {sizes.Sum(s=> s.Diff)} MB");
        sb.AppendLineAndConsole("\nCleanup");

        var dirsToDelete = sizes.Select(x => x.Directory).Distinct();
        foreach (var directory in dirsToDelete)
        {
            var dirInfo = new DirectoryInfo(directory);
            if (!dirInfo.Exists || directory == o.TempOutDir)
                continue;
            
            var empty =  !dirInfo.EnumerateFiles().Any();
            if (empty)
            {
                if (!o.TestMode)
                {
                    dirInfo.Delete();
                }
                sb.AppendLineAndConsole($"Deleted: {dirInfo.Name}");
            }
            else
            {
                sb.AppendLineAndConsole($"Directory not empty: {dirInfo.FullName}");
            }
        }

        //deleting empty
        foreach (var dirInfo in new DirectoryInfo(o.TempOutDir).GetDirectories())
        {
            var empty =  !dirInfo.EnumerateFiles().Any();
            if (empty)
            {
                if (!o.TestMode)
                {
                    dirInfo.Delete();
                }
                sb.AppendLineAndConsole($"Deleted: {dirInfo.Name}");
            }
        }
        sb.AppendLineAndConsole("=====================");
    }

    public static void List(Options o, IEnumerable<string> videoFiles)
    {
        var sb = new StringBuilder();
    
        foreach (var file in videoFiles)
        {
            sb.AppendLine(file);
        }

        FileHlp.SaveFile(".", sb, "listing.txt");
    }
    
}