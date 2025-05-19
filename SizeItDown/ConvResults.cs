public class ConvResults
{
    public int ImagesFilesConvertedCnt { get; set; }
    public int ImagesFilesResizedCnt { get; set; }
    public long ImagesTotalSizeBefore { get; set; }
    public long ImagesTotalSizeAfter { get; set; }
    
    public int VideosCount { get; set; }
    public int VideosProcessed { get; set; }
    public int VideosFailed { get; set; }
    public int VideosReplaced { get; set; }
    public int VideosBiggerAfter { get; set; }
    
    public long VideosTotalSizeBefore { get; set; }
    public long VideosTotalSizeAfter { get; set; }
}