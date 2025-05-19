using System.Diagnostics;
using System.Reflection;
using CommandLine;
using SizeItDown.Generators;

await Parser.Default.ParseArguments<Options>(args)
    .WithParsedAsync<Options>(Start);

Console.WriteLine("Finished");
return;


async Task Start(Options o)
{
    Console.WriteLine("Started..");
    var stopwatch = Stopwatch.StartNew();
    MyContext.Instance.IsTestMode = o.TestMode;

    if (!Directory.Exists(o.InputDir))
        throw new DirectoryNotFoundException(o.InputDir);
    if (!Directory.Exists(o.OutputDir))
        o.OutputDir = Path.GetTempPath();
    
    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
    o.OutputDir = Path.Combine(o.OutputDir, timestamp);
    Directory.CreateDirectory(o.OutputDir);
    
    var allFilesList = Directory.EnumerateFiles(o.InputDir, "*.*", SearchOption.AllDirectories).ToList();
    var inputFolderSize1 = allFilesList.Sum(x => new FileInfo(x).Length);
    
    var mediaFiles = allFilesList
        .Where(x => Const.AllExtensions.Contains(Path.GetExtension(x), StringComparer.OrdinalIgnoreCase))
        .ToList();

    long deletedMotionFilesSize = o.CleanMotionFiles
        ? Commands.CleanMotionFiles(o, mediaFiles)
        : 0;
    
    var logFile = $"{o.InputDir}\\conversionLog_{timestamp}.txt";
    var sb = new MyStringBuilder(logFile);

    //need to remove all motion files
    allFilesList = null;
    var deleted = mediaFiles.RemoveAll(x => Path.GetExtension(x).Equals(".mp", StringComparison.CurrentCultureIgnoreCase));
    var videoFiles = mediaFiles.Where(x => Const.VideoExtensions.Contains(Path.GetExtension(x), StringComparer.OrdinalIgnoreCase)).ToList();

    if (o.List)
    {
        Console.WriteLine("Mode: List");
        Commands.List(o, mediaFiles);
    }
    else if (o.Replace)
    {
        Console.WriteLine("Mode: Replace");
        Commands.Replace(o, videoFiles, sb);
    }
    else
    {
        Console.WriteLine("Mode: Generate");
        //SaveResourceFileOnDisk("cropAndconvertImagesToWebP.py");

        //WebPGenerator.Generate(o, deletedFileSize);
        var webPConverter = new WebPConverter(sb, deletedMotionFilesSize);
        webPConverter.Convert(o, mediaFiles.Where(x => Const.ImagesExtensions.Contains(Path.GetExtension(x), StringComparer.OrdinalIgnoreCase)).ToList());
        webPConverter.DescribeConversionsResults();
        sb.AppendToFile();

        //Hanbrake.Generate(o, videoFiles);
        var runner = new HandbrakeRunner();
        await runner.Run(o, videoFiles, sb);
        sb.AppendToFile();
        
        //Hanbrake.GenerateReplaceFile(o);
        Commands.Replace(o, videoFiles, sb);
        sb.AppendToFile();
        
        allFilesList = Directory.EnumerateFiles(o.InputDir, "*.*", SearchOption.AllDirectories).ToList();
        var inputFolderSize2 = allFilesList.Sum(x => new FileInfo(x).Length);
        
        sb.AppendLineAndConsole($"Folder size at START: {inputFolderSize1.ToKBStr()}");
        sb.AppendLineAndConsole($"Folder size at END  : {inputFolderSize2.ToKBStr()}");
        sb.AppendLineAndConsole($"Saved : {(inputFolderSize1-inputFolderSize2).ToKBStr()}");
        sb.AppendLineAndConsole($"Execution Time: {Math.Round(stopwatch.Elapsed.TotalSeconds, 2)} s");
        sb.AppendToFile();
    }
}

