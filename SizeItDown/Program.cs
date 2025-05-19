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

    // DIRECTORIES - creating dir structure in temp folder 
    foreach (var inputFile in allFilesList)
    {
        var outPutFile = inputFile.Replace(o.InputDir, o.TempOutDir);
        FileHlp.EnsureDirStructure(outPutFile);
    }

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
    else if (o.ManualReplace)
    {
        Console.WriteLine("Mode: ManualReplace");
        sb = new MyStringBuilder($"{o.InputDir}\\replacementLog_{timestamp}.txt");
        Commands.Replace(o, videoFiles, sb);
    }
    else
    {
        Console.WriteLine("Mode: Conversion");
        //SaveResourceFileOnDisk("cropAndconvertImagesToWebP.py");
        double imagesConvertedIn = 0;
        var results = new ConvResults();

        // IMAGES
        //WebPGenerator.Generate(o, deletedFileSize);
        if (o.DoImages)
        {
            var imageFiles = mediaFiles.Where(x => Const.ImagesExtensions.Contains(Path.GetExtension(x), StringComparer.OrdinalIgnoreCase)).ToList();
            var webPConverter = new WebPConverter(results, sb, deletedMotionFilesSize);
            results = await webPConverter.Convert(o, imageFiles);
            webPConverter.DescribeConversionsResults();
            sb.AppendToFile();
            imagesConvertedIn = stopwatch.Elapsed.TotalSeconds;
        }
        

        // VIDEOS
        //Hanbrake.Generate(o, videoFiles);
        if (o.DoVideos)
        {
            sb = new MyStringBuilder($"{o.InputDir}\\videosConversionLog_{timestamp}.txt");
            var runner = new HandbrakeRunner(results, sb);
            results = await runner.Run(o, videoFiles);
            sb.AppendToFile();
        }

        // REPLACE FILES
        //Hanbrake.GenerateReplaceFile(o);
        if (o.AutoReplace)
        {
            sb = new MyStringBuilder($"{o.InputDir}\\replacementLog_{timestamp}.txt");
            Commands.Replace(o, videoFiles, sb);
            sb.AppendToFile();
        }

        allFilesList = Directory.EnumerateFiles(o.InputDir, "*.*", SearchOption.AllDirectories).ToList();
        var inputFolderSize2 = allFilesList.Sum(x => new FileInfo(x).Length);
        
        sb = new MyStringBuilder($"{o.InputDir}\\summaryLog_{timestamp}.txt");
        if (o.DoImages)
        {
            sb.AppendLineAndConsole($"====================================================");
            sb.AppendLineAndConsole($"Images converted: {results.ImagesFilesConvertedCnt}");
            sb.AppendLineAndConsole($"Images resized: {results.ImagesFilesResizedCnt}");
            var imgReduction = (results.ImagesTotalSizeBefore - results.ImagesTotalSizeAfter).ToMBStr();
            var imgPerc = results.ImagesTotalSizeBefore.PercentChange(results.ImagesTotalSizeAfter);
            sb.AppendLineAndConsole(
                $"Images size before: {results.ImagesTotalSizeBefore.ToMBStr()}, after: {results.ImagesTotalSizeAfter.ToMBStr()}, diff: {imgReduction} ({imgPerc}%)");
            sb.AppendLineAndConsole($"Executed in: {Math.Round(imagesConvertedIn / 60, 2)} min");
        }

        if (o.DoVideos)
        {
            sb.AppendLineAndConsole($"====================================================");
            sb.AppendLineAndConsole($"Videos found: {results.VideosCount}");
            sb.AppendLineAndConsole($"Videos failed: {results.VideosFailed}");
            sb.AppendLineAndConsole($"Videos replaced: {results.VideosReplaced}");
            sb.AppendLineAndConsole($"Videos larger after: {results.VideosBiggerAfter}");
            sb.AppendLineAndConsole($"====================================================");
        }

        if (o.AutoReplace)
        {
            //sb.AppendLineAndConsole($"Folder size at START: {results.ImagesTotalSizeBefore.ToMBStr()}");
            sb.AppendLineAndConsole($"Folder size at START: {inputFolderSize1.ToMBStr()}");
            sb.AppendLineAndConsole($"Folder size at END  : {inputFolderSize2.ToMBStr()}");
            sb.AppendLineAndConsole($"Saved : {(inputFolderSize1 - inputFolderSize2).ToMBStr()}");
        }
        sb.AppendLineAndConsole($"Executed in: {Math.Round(stopwatch.Elapsed.TotalMinutes, 2)} min");
        sb.AppendLineAndConsole($"====================================================");

        sb.AppendToFile();
    }
}