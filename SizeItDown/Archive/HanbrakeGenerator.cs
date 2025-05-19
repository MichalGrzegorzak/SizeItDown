namespace SizeItDown.Generators;

public static class Hanbrake
{
    public static void Generate(Options o, IEnumerable<string> videoFiles)
    {
        var sb = new StringBuilder();
        sb.AppendLine("@ECHO OFF");
        sb.AppendLine($"SET INPUT={o.InputDir}");
        sb.AppendLine($"SET PRESET=\"{o.VideoPreset}\"");
        sb.AppendLine("""SET CLI=@HandBrakeCLI --preset-import-file %PRESET%""");
        sb.AppendLine("setlocal");
        sb.AppendLine("""for /f "tokens=2 delims==" %%I in ('"wmic os get localdatetime /value"') do set datetime=%%I""");
        sb.AppendLine("""set LOG=%INPUT%\2_handbrake_%datetime:~0,8%_%datetime:~8,4%.txt""");
        sb.AppendLine("@ECHO ON");
        sb.AppendLine("");

        var cnt = videoFiles.Count();
        int idx = 1;

        foreach (var file in videoFiles)
        {
            var target = file.Replace(o.InputDir, o.TempOutDir);
            var shorterFilePath = file.Replace(o.InputDir, "");
            sb.AppendLine($"@ECHO File: {idx++}/{cnt} - {shorterFilePath}");

            var line = string.Format($"%CLI% -i \"{file}\" -o \"{target}\" >> \"%LOG%\" 2>&1", target);
            sb.AppendLine(line);
            sb.AppendLine("");

            FileHlp.EnsureDirStructure(target);
        }

        FileHlp.SaveFile(".", sb, "2_start_video_conversion.bat");
    }

    public static void GenerateReplaceFile(Options o)
    {
        var sb = new StringBuilder();
        sb.AppendLine("@ECHO OFF");
        sb.AppendLine($"SET INPUT=\"{o.InputDir}\"");
        sb.AppendLine($"SET OUT=\"{o.TempOutDir}\"");
        sb.AppendLine($"SET VID_PRESET=\"{o.VideoPreset}\"");
        sb.AppendLine($"SET IMG_QUALITY={o.ImageQuality}");
        sb.AppendLine($"SET IMG_CROP_TO={o.ImageMaxWidth}");
        sb.AppendLine("@ECHO ON");
        sb.AppendLine("@ECHO !!! It will replace all the matching videos in INPUT, that it finds in OUT !!!");
        //rem -t = testmode
        //rem -r = replace

        //%CLI% -i \"%INPUT%{0}\" -o \"{1}\ > %LOG% 2>>&1
        string testMode = o.TestMode ? "-t" : "";
        string replaceMode = "-r";
        //string template = $"SizeItDown -i %INPUT% -o %OUT% -p %VID_PRESET% -q %IMG_QUALITY% -c %IMG_CROP_TO% {replaceMode} {testMode}";
        string template = $"SizeItDown -i %INPUT% -o %OUT% {replaceMode} {testMode}";
        sb.AppendLine(template);

        FileHlp.SaveFile(".", sb, "replace_converted_videos.bat");
    }
}