using System.Diagnostics;

namespace SizeItDown.Generators;

public class HandbrakeRunner
{
    object writeLock = new object();

    public async Task Run(Options o, List<string> videoFiles, MyStringBuilder sb)
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
        string logFilePath = $"handbrake_log_{timestamp}.txt";

        //aquiring logFilePath fails
        //await Parallel.ForEachAsync(videoFiles, async (inputFile, cancellationToken) =>
        //
        int idx = 1;
        var cnt = videoFiles.Count();
        foreach (var inputFile in videoFiles)
        {
            var outPutFile = inputFile.Replace(o.InputDir, o.OutputDir);
            FileHlp.EnsureDirStructure(outPutFile);
            string arguments = $"--preset-import-file \"{o.VideoPreset}\" -i \"{inputFile}\" -o \"{outPutFile}\"";

            var shorterFilePath = inputFile.Replace(o.InputDir, "");
            //lock (writeLock)
            {
                sb.AppendLineAndConsole($"Processing: {idx++}/{cnt} - {shorterFilePath}");
            }

            await RunHandBrakeAsync(arguments, logFilePath, sb);
        };
        
        sb.AppendLineAndConsole($"All jobs completed.");
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

        process.OutputDataReceived += async (s, e) =>
        {
            if (e.Data != null)
            {
                lock (writeLock)
                {
                    logWriter.WriteLine(e.Data);
                }
            }
        };
        process.ErrorDataReceived += async (s, e) =>
        {
            if (e.Data != null)
            {
                lock (writeLock)
                {
                    logWriter.WriteLine("[ERROR] " + e.Data);
                }
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();
        await logWriter.WriteLineAsync($"[Exit Code: {process.ExitCode}]\n");
    }

    
}