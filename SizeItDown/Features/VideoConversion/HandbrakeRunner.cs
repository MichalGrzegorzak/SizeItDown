using System.Diagnostics;
using Ardalis.GuardClauses;

namespace SizeItDown.Generators;

public class HandbrakeRunner
{
    private readonly MyStringBuilder _sb;
    private readonly ConvResults _results;
    object writeLock = new object();

    public HandbrakeRunner(ConvResults results, MyStringBuilder sb)
    {
        _sb = sb;
        _results = results;
    }
    public async Task<ConvResults> Run(Options o, List<string> videoFiles)
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
        string logFilePath = $"handbrake_log_{timestamp}.txt";
        
        var cnt = videoFiles.Count();
        _results.VideosCount = cnt;
        _sb.AppendLineAndConsole($"\nStarting video conversion, files count: {cnt}");
        _sb.AppendLineAndConsole($"\nUsing Handbrake video preset: {o.VideoPreset}");
        _sb.AppendLineAndConsole($"Handbrake logs in: {logFilePath}\n");

        //aquiring logFilePath fails
        //await Parallel.ForEachAsync(videoFiles, async (inputFile, cancellationToken) =>
        //
        int idx = 1;

        foreach (var inputFile in videoFiles)
        {
            var outPutFile = inputFile.Replace(o.InputDir, o.TempOutDir);
            string arguments = $"--preset-import-file \"{o.VideoPreset}\" -i \"{inputFile}\" -o \"{outPutFile}\"";

            await RunHandBrakeAsync(arguments, logFilePath, _sb);
            
            var shorterFilePath = inputFile.Replace(o.InputDir, "");
            var sizes = new FileSizes(new FileInfo(inputFile), new FileInfo(outPutFile));

            //lock (writeLock)
            {
                //_sb.AppendLineAndConsole($"Processing: {idx++}/{cnt} - {shorterFilePath}");
                string biggerOrsmaller = sizes.Input > sizes.Output ? "smaller" : "larger";
                _sb.AppendLineAndConsole($"Processing: {idx++}/{cnt} - {shorterFilePath}, before: {sizes.Input.ToMBStr()}, after: {sizes.Output.ToMBStr()}, result: {biggerOrsmaller}");
                _results.VideosProcessed++;
                _results.VideosTotalSizeBefore += sizes.Input;
                _results.VideosTotalSizeAfter += sizes.Output;

                if (sizes.Output < sizes.Input)
                {
                    //File.Move(outPutFile, inputFile, true);
                    if (o.AutoReplace && !MyAppContext.Instance.IsTestMode)
                    {
                        _results.VideosReplaced++;
                        await FileHlp.ReplaceFileAsync(outPutFile, inputFile);
                    }
                       
                }
                else
                {
                    _results.VideosBiggerAfter++;
                    if(o.AutoReplace && !MyAppContext.Instance.IsTestMode)
                        File.Delete(outPutFile);
                }
            }
        };
        
        _sb.AppendLineAndConsole($"\nAll jobs completed.");
        return _results;
    }

    async Task RunHandBrakeAsync(string arguments, string logFilePath, MyStringBuilder sb)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "HandBrakeCLI.exe",
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            }
        };

        await using StreamWriter logWriter = new StreamWriter(logFilePath, append: true);
        await logWriter.WriteLineAsync($"\n[{DateTime.Now}] Running: HandBrakeCLI.exe {arguments}");

        bool encodingStarted = false;

        process.OutputDataReceived += async (s, e) =>
        {
            if (e.Data == null || string.IsNullOrEmpty(e.Data)) 
                return;
            
            lock (writeLock)
            {
                logWriter.WriteLine(e.Data);
                if (!encodingStarted && e.Data.StartsWith("Encoding: task"))
                    encodingStarted = true;
            }
        };
        process.ErrorDataReceived += async (s, e) =>
        {
            if (e.Data == null) 
                return;
            lock (writeLock)
                logWriter.WriteLine("[ERROR] " + e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();
        await logWriter.WriteLineAsync($"[Exit Code: {process.ExitCode}]\n");

        if (!encodingStarted)
        {
            lock (writeLock)
            {
                _results.VideosFailed++;
                Debugger.Break();
            }
        }
    }

    
}