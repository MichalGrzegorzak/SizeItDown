using System.Diagnostics;
using System.Reflection;
using Ardalis.GuardClauses;
using CommandLine;
using SizeItDown.Generators;

// first run will add it for you
FileHlp.SaveResourceFileOnDisk("start_conversion.bat");

await Parser.Default.ParseArguments<Options>(args)
    .WithParsedAsync<Options>(Start);

Console.WriteLine("Finished");
return;


async Task Start(Options o)
{
    Console.WriteLine("Started..");
    var stopwatch = Stopwatch.StartNew();
    MyAppContext.Instance.IsTestMode = o.TestMode;

    //adding default video preset, but you should use your own
    FileHlp.SaveResourceFileOnDisk("HandbrakeVideoPreset.json");

    Guard.Against.Default(File.Exists("./HandbrakeCLI.exe"), null, "HandbrakeCLI.exe not found. You need to download it, and place beside this app.");
    //Guard.Against.Default( File.Exists("./HandbrakeVideoPreset.json"), null, "HandbrakeVideoPreset.json not found");

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

    var sb = new MyStringBuilder($"{o.InputDir}\\imagesConversionLog_{timestamp}.txt");
    if (o.ReplaceMode)
    {
        Console.WriteLine("Mode: Big replacement");
        sb = new MyStringBuilder($"{o.InputDir}\\replacementLog_{timestamp}.txt");
        Commands.Replace(o, mediaFiles, sb);
    }

    // Motion files handling, it will rename them to videos (mp4), which will be later converted.
    // Question is do you want to get rid of them or not ?
    long deletedMotionFilesSize = 0;
    deletedMotionFilesSize = o.CleanMotionFiles
        ? Commands.CleanMotionFiles(o, mediaFiles)
        : 0;
    //remove all motion files as no longer needed
    var deleted = mediaFiles.RemoveAll(x => Path.GetExtension(x).Equals(".mp", StringComparison.CurrentCultureIgnoreCase));

    if (o.ListMode)
    {
        Console.WriteLine("Mode: List");
        Commands.List(o, mediaFiles);
    }
    else
    {
        Console.WriteLine("Mode: Conversion");

        // DIRECTORIES - creating dir structure in temp folder 
        foreach (var inputFile in allFilesList)
        {
            var outPutFile = inputFile.Replace(o.InputDir, o.TempOutDir);
            FileHlp.EnsureDirStructure(outPutFile);
        }

        allFilesList = null; //clears some mem if many files

        //SaveResourceFileOnDisk("cropAndconvertImagesToWebP.py");
        double imagesConvertedIn = 0;
        var results = new ConvResults();

        // IMAGES
        //WebPGenerator.Generate(o, deletedFileSize);
        if (o.DoImages)
        {
            Console.WriteLine("Images conversion started");
            var imageFiles = mediaFiles.Where(x => Const.ImagesExtensions.Contains(Path.GetExtension(x), StringComparer.OrdinalIgnoreCase)).ToList();
            var webPConverter = new ImageConverter(results, sb, deletedMotionFilesSize);
            results = await webPConverter.Convert(o, imageFiles);
            webPConverter.DescribeConversionsResults();
            sb.AppendToFile();
            imagesConvertedIn = stopwatch.Elapsed.TotalSeconds;
        }

        // VIDEOS
        var videoFiles = mediaFiles.Where(x => Const.VideoExtensions.Contains(Path.GetExtension(x), StringComparer.OrdinalIgnoreCase)).ToList();

        //Hanbrake.Generate(o, videoFiles);
        if (o.DoVideos)
        {
            Console.WriteLine("Videos conversion started");
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

        if (o.ReplaceMode || o.AutoReplace)
        {
            //sb.AppendLineAndConsole($"Folder size at START: {results.ImagesTotalSizeBefore.ToMBStr()}");
            sb.AppendLineAndConsole($"Folder size BEFORE: {inputFolderSize1.ToMBStr()}");
            sb.AppendLineAndConsole($"Folder size AFTER : {inputFolderSize2.ToMBStr()}");
            sb.AppendLineAndConsole($"Saved space : {(inputFolderSize1 - inputFolderSize2).ToMBStr()}");
        }

        sb.AppendLineAndConsole($"Executed in: {Math.Round(stopwatch.Elapsed.TotalMinutes, 2)} min");
        sb.AppendLineAndConsole($"====================================================");
        sb.AppendToFile();
    }
}