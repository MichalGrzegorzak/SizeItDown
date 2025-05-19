namespace SizeItDown.Generators;

public class WebPGenerator
{
    public static void Generate(Options o, long deletedFileSize)
    {
        var deleted = deletedFileSize / (1024 * 1024);
        
        var sb = new StringBuilder();
        sb.AppendLine("@ECHO OFF");
        //sb.AppendLine("rem Deleted MB: " + );
        sb.AppendLine("");
        sb.AppendLine($"SET INPUT=\"{o.InputDir}\"");
        sb.AppendLine($"SET IMG_QUALITY={o.ImageQuality}");
        sb.AppendLine($"SET IMG_CROP_TO={o.ImageCropTo}");
        sb.AppendLine("""SET CLI=HandBrakeCLI --preset-import-file "%PRESET%""");
        sb.AppendLine("");
        
        string template = $"python cropAndconvertImagesToWebP.py -i %INPUT% -q %IMG_QUALITY% -c %IMG_CROP_TO% -d {deleted:F1} -t {(o.TestMode ? 1 : 0).ToString()}";
        sb.AppendLine(template);
        
        FileHlp.SaveFile(".", sb, "1_crop_and_convert_images_to_webP.bat");
    }
}