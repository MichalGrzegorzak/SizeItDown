using System.Diagnostics;
using Nito.AsyncEx;
using SizeItDown.Features.Core;
using Xabe.FFmpeg;

namespace SizeItDown.Generators;

public class HandbrakeRunner
{
    private readonly MyStringBuilder _sb;
    private readonly ConvResults _results;
    private static readonly AsyncLock _asyncLock = new AsyncLock();

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
        
        //acquiring logFilePath fails
        //await Parallel.ForEachAsync(videoFiles, async (inputFile, cancellationToken) =>
        //
        int myIdx = 1;

        //foreach (var inputFile in videoFiles)
        var processors = 3; //Environment.ProcessorCount /4;
        
        await Parallel.ForEachAsync(videoFiles, new ParallelOptions { MaxDegreeOfParallelism = processors }, async (inputFile, _) =>
        {
            int idx = Interlocked.Increment(ref myIdx) - 1;
            var localLog = new List<string>();
            
            var outPutFile = inputFile.Replace(o.InputDir, o.TempOutDir);
            var shorterFilePath = inputFile.Replace(o.InputDir, "");
            
            string arguments = $"--preset-import-file \"{o.VideoPreset}\" -i \"{inputFile}\" -o \"{outPutFile}\"";
            string beginning = $"Video {idx}/{cnt}";

            string codec = string.Empty;
            try
            {
                var mediaInfo = await FFmpeg.GetMediaInfo(inputFile);
                var videoStream = mediaInfo.VideoStreams.First();
                codec = videoStream.Codec;  // e.g., "hevc"
                long bitrate = videoStream.Bitrate/(1024*1024);
                string dimension = $"{videoStream.Width}x{videoStream.Height}";
                _sb.AppendLineAndConsole($"{beginning}, {shorterFilePath} => codec:{codec}, bitrate:{bitrate}, dimensions:{dimension}");

                if (codec == "h264" || codec == "vp9" || codec == "mss2")
                {
                    _sb.AppendLineAndConsole($"{beginning}, SKIPPING codec: {codec}");
                    _results.VideosSkipped++;
                    return;
                }
            }
            catch (Exception e)
            {
                _sb.AppendLineAndConsole($"{beginning}, BAD FILE? - EXCEPTION: {e.Message}");
                _results.VideosBiggerAfter++;
                return;
            }

            await RunHandBrakeAsync(arguments, localLog);

            var outputFileInfo = new FileInfo(outPutFile);
            if (!outputFileInfo.Exists)
            {
                _results.VideosFailed++;
                _sb.AppendLineAndConsole($"{beginning}, processing FAILED,  skipping it.");
                return;
            }

            var sizes = new FileSizes(new FileInfo(inputFile), outputFileInfo);

            using (await _asyncLock.LockAsync())
            {
                await File.AppendAllLinesAsync(logFilePath, localLog);
                
                //_sb.AppendLineAndConsole($"Processing: {idx++}/{cnt} - {shorterFilePath}");
                string biggerOrsmaller = sizes.Input > sizes.Output ? "less" : "more";
                _sb.AppendLineAndConsole($"{beginning}, {shorterFilePath} bef: {sizes.Input.ToKBStr()}, aft: {sizes.Output.ToKBStr()}, res: {biggerOrsmaller}");
                _results.VideosProcessed++;
                _results.VideosTotalSizeBefore += sizes.Input;
                _results.VideosTotalSizeAfter += sizes.Output;

                outputFileInfo = outputFileInfo.AddPostfix("_[c]"); //marking to not process this file again
                outPutFile = outputFileInfo.FullName;
                
                if (!_results.CodecResults.ContainsKey(codec))
                    _results.CodecResults.Add(codec, new ResultTracker());
                
                if (sizes.Output < sizes.Input)
                {
                    if (o.AutoReplace && !MyAppContext.Instance.IsTestMode)
                    {
                        _results.VideosReplaced++;
                        await FileHlp.ReplaceFileAsync(outPutFile, inputFile);
                        //File.Move(outPutFile, inputFile, true);
                    }

                    _results.CodecResults[codec].Positive++;
                }
                else
                {
                    _results.CodecResults[codec].Negative++;
                    _results.VideosBiggerAfter++;
                    if (o.AutoReplace && !MyAppContext.Instance.IsTestMode)
                    {
                        File.Delete(outPutFile);
                        new FileInfo(inputFile).AddPostfix("_[c]"); //marking to not process this file again
                    }
                }

                //idx++;
            }
        });
        
        _sb.AppendLineAndConsole($"\nAll video jobs completed.");
        return _results;
    }

    async Task RunHandBrakeAsync(string arguments, List<string> localLog)
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

        //await using StreamWriter logWriter = new StreamWriter(logFilePath, append: true);
        //await logWriter.WriteLineAsync($"\n[{DateTime.Now}] Running: HandBrakeCLI.exe {arguments}");

        process.OutputDataReceived += async (s, e) =>
        {
            if (e.Data == null || string.IsNullOrEmpty(e.Data)) 
                return;
            
            //lock (writeLock)
                localLog.Add(e.Data);
                //logWriter.WriteLine(e.Data);
        };
        process.ErrorDataReceived += async (s, e) =>
        {
            if (e.Data == null) 
                return;
            //lock (writeLock)
                localLog.Add("[ERROR] " + e.Data);
                //logWriter.WriteLine("[ERROR] " + e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();
        localLog.Add($"[Exit Code: {process.ExitCode}]\n");
        //await logWriter.WriteLineAsync($"[Exit Code: {process.ExitCode}]\n");
    }
}