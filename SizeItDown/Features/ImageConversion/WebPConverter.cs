using System.Diagnostics;
using ImageMagick;

namespace SizeItDown.Generators;

public class WebPConverter
{
    List<ConversionInfo> _conversions = new();
    private long _deletedFileSize;
    private readonly ConvResults _results;
    private MyStringBuilder sb;
    private static readonly object _lock = new object();

    public WebPConverter(ConvResults results, MyStringBuilder sb, long deletedFileSize)
    {
        _results = results;
        this.sb = sb;
        _deletedFileSize = deletedFileSize;
    }

    public async Task<ConvResults> Convert(Options o, List<string> imagePaths)
    {
        _conversions = new List<ConversionInfo>();

        int idx = 1;
        var cnt = imagePaths.Count();
        Parallel.ForEach(imagePaths, async path =>
        {
            var deleteOriginalImage = o.AutoReplace && !MyAppContext.Instance.IsTestMode;
            var savePath = o.AutoReplace
                ? path
                : path.Replace(o.InputDir, o.TempOutDir);                
            
            using var magick = new MagickImageBuilder(path, sb);
            var ci = await magick.Resize(o.ImageCropTo)
                .Convert(MagickFormat.WebP, o.ImageQuality)
                .SaveAsync(savePath, deleteOrg: deleteOriginalImage);
            
            var line = $"{idx}/{cnt}. Converted {ci.FileName}, size: {ci.SizeBefore.ToKBStr()} -> {ci.SizeAfter.ToKBStr()}, reduced: {ci.Reduction} KB ({ci.Percentage})";
            lock (_lock)
            {
                _conversions.Add(ci);
                sb.AppendLineAndConsole(line);
                idx++;
                
                if(ci.SizeAfter > ci.SizeBefore)
                    Debugger.Break();
                
                //keeping stats
                if (ci.WasResized)
                    _results.ImagesFilesResizedCnt++;
                _results.ImagesFilesConvertedCnt++;
                _results.ImagesTotalSizeBefore += ci.SizeBefore;
                _results.ImagesTotalSizeAfter += ci.SizeAfter;
            }
        });

        return _results;
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