using System.Diagnostics;
using ImageMagick;
using SizeItDown.Features.Core;

namespace SizeItDown.Generators;

public class ImageConverter
{
    List<ConversionInfo> _conversions = new();
    private long _deletedFileSize;
    private readonly ConvResults _results;
    private MyStringBuilder sb;
    private static readonly object _lock = new object();

    public ImageConverter(ConvResults results, MyStringBuilder sb, long deletedFileSize)
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
        var processors = Environment.ProcessorCount /2;
        
        await Parallel.ForEachAsync(imagePaths, new ParallelOptions { MaxDegreeOfParallelism = processors }, async (path, _) =>
        {
            var imgFormat = o.ImageConvTo.ToLower() == "webp" ? MagickFormat.WebP : MagickFormat.Avif;
            
            var deleteOriginalImage = o.AutoReplace && !MyAppContext.Instance.IsTestMode;
            var savePath = o.AutoReplace
                ? path
                : path.Replace(o.InputDir, o.TempOutDir);                
            
            using var magick = new MagickImageBuilder(path, sb);
            var ci = await magick.Resize(o.ImageMaxWidth)
                .Convert(imgFormat, o.ImageQuality)
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
        
        sb.AppendLineAndConsole("=============================");
        sb.AppendLineAndConsole("Total size before: " + before.ToMBStr());
        sb.AppendLineAndConsole("Total size after: " + after.ToMBStr());
        sb.AppendLineAndConsole($"Total size deleted: {_deletedFileSize.ToMBStr()}");
        sb.AppendLineAndConsole($"Total size reduction: {reduction.ToMBStr()}, perc: ({percent:F1}%)");
        sb.AppendLineAndConsole("==============================");
    }
}