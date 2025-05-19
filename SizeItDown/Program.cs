using System.Diagnostics;
using System.Reflection;
using Ardalis.GuardClauses;
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
    MyAppContext.Instance.IsTestMode = o.TestMode;
    
    FileHlp.SaveResourceFileOnDisk("HandbrakeVideoPreset.json");
    
    bool cliExists = File.Exists("./HandbrakeCLI.exe");
    bool presetExists = File.Exists("./HandbrakeVideoPreset.json");
    Guard.Against.Default(cliExists, null, "HandbrakeCLI.exe not found. You need to download it, and place beside this app.");
    Guard.Against.Default(presetExists, null, "HandbrakeVideoPreset.json not found");

    if (!Directory.Exists(o.InputDir))
        throw new DirectoryNotFoundException(o.InputDir);
    if (!Directory.Exists(o.TempOutDir))
        o.TempOutDir = Path.GetTempPath();
    
    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
    o.TempOutDir = Path.Combine(o.TempOutDir, timestamp);
    Directory.CreateDirectory(o.TempOutDir);
    
    var allFilesList = Directory.EnumerateFiles(o.InputDir, "*.*", SearchOption.AllDirectories).ToList();
    var inputFolderSize1 = allFilesList.Sum(x => new FileInfo(x).Length);
    
    var mediaFiles = allFilesList
        .Where(x => Const.AllExtensions.Contains(Path.GetExtension(x), StringComparer.OrdinalIgnoreCase))
        .ToList();

    long deletedMotionFilesSize = o.CleanMotionFiles
        ? Commands.CleanMotionFiles(o, mediaFiles)
        : 0;
    
    var sb = new MyStringBuilder($"{o.InputDir}\\imagesConversionLog_{timestamp}.txt");

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
        sb = new MyStringBuilder($"{o.InputDir}\\replacementLog_{timestamp}.txt");
        Commands.Replace(o, videoFiles, sb);
    }
    else
    {
        Console.WriteLine("Mode: Generate");
        //SaveResourceFileOnDisk("cropAndconvertImagesToWebP.py");

        //WebPGenerator.Generate(o, deletedFileSize);
        var webPConverter = new WebPConverter(sb, deletedMotionFilesSize);
        var results = webPConverter.Convert(o, mediaFiles.Where(x => Const.ImagesExtensions.Contains(Path.GetExtension(x), StringComparer.OrdinalIgnoreCase)).ToList());
        webPConverter.DescribeConversionsResults();
        sb.AppendToFile();

        //Hanbrake.Generate(o, videoFiles);
        sb = new MyStringBuilder($"{o.InputDir}\\videosConversionLog_{timestamp}.txt");
        var runner = new HandbrakeRunner(sb, results);
        results = await runner.Run(o, videoFiles);
        sb.AppendToFile();
        
        //Hanbrake.GenerateReplaceFile(o);
        sb = new MyStringBuilder($"{o.InputDir}\\replacementLog_{timestamp}.txt");
        Commands.Replace(o, videoFiles, sb);
        sb.AppendToFile();
        
        allFilesList = Directory.EnumerateFiles(o.InputDir, "*.*", SearchOption.AllDirectories).ToList();
        var inputFolderSize2 = allFilesList.Sum(x => new FileInfo(x).Length);
        
        sb = new MyStringBuilder($"{o.InputDir}\\summaryLog_{timestamp}.txt");
        sb.AppendLineAndConsole($"Folder size at START: {inputFolderSize1.ToMBStr()}");
        sb.AppendLineAndConsole($"Folder size at END  : {inputFolderSize2.ToMBStr()}");
        sb.AppendLineAndConsole($"Saved : {(inputFolderSize1-inputFolderSize2).ToMBStr()}");
        sb.AppendLineAndConsole($"Execution Time: {Math.Round(stopwatch.Elapsed.TotalMinutes, 2)} min");
        sb.AppendToFile();
    }
}