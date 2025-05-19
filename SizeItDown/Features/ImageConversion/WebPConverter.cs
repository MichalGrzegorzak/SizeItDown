using ImageMagick;

namespace SizeItDown.Generators;

public class WebPConverter
{
    List<ConversionInfo> _conversions = new();
    private long _deletedFileSize;
    private MyStringBuilder sb;
    private static readonly object _lock = new object();

    public WebPConverter(MyStringBuilder sb, long deletedFileSize)
    {
        this.sb = sb;
        _deletedFileSize = deletedFileSize;
    }

    public void Convert(Options o, List<string> imagePaths)
    {
        _conversions = new List<ConversionInfo>();

        int idx = 1;
        var cnt = imagePaths.Count();
        Parallel.ForEach(imagePaths, path =>
        {
            using var magick = new MagickImageBuilder(path, sb);
            var ci = magick.Resize(o.ImageCropTo)
                .Convert(MagickFormat.WebP, o.ImageQuality)
                .Save(null);
            
            var line = $"{idx}/{cnt}. Converted {ci.FileName}, size: {ci.SizeBefore.ToKBStr()} -> {ci.SizeAfter.ToKBStr()}, reduced: {ci.Reduction} KB ({ci.Percentage})";
            lock (_lock)
            {
                _conversions.Add(ci);
                sb.AppendLineAndConsole(line);
                idx++;
            }
        });
    }

    public void DescribeConversionsResults()
    {
        var before = _conversions.Sum(x => x.SizeBefore) + _deletedFileSize;
        var after = _conversions.Sum(x => x.SizeAfter);
        var reduction = before - after;
        var percent = before.PercentChange(after)*-1;
        
        sb.AppendLine("=============================");
        sb.AppendLine("Total size before: " + before.ToKBStr());
        sb.AppendLine("Total size after: " + after.ToKBStr());
        sb.AppendLine($"Total size deleted: {_deletedFileSize.ToKBStr()}");
        sb.AppendLine($"Total size reduction: {reduction.ToKBStr()}, perc: ({percent:F1})");
        sb.AppendLine("==============================");
    }
}