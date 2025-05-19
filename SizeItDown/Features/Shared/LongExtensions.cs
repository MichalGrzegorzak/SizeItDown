namespace SizeItDown.Generators;

public static class LongExtensions
{
    public static decimal ToKB(this long input) => Math.Round((decimal)input / 1024, 1);
    public static decimal ToMB(this long input) => Math.Round((decimal)input / (1024*1024), 1);
    public static string ToKBStr(this long input) => input.ToKB().ToString("F1") + " KB";
    public static string ToMBStr(this long input) => input.ToMB().ToString("F1") + " MB";
    
    // public static decimal ToUnivSize(this long input)
    // {
    //     if (input > 1_000_000)
    //         input.ToMB()
    //     return Math.Round((decimal)input / (1024 * 1024), 1);
    // }

    public static decimal PercentChange(this long before, long after)
    {
        if (before == 0)
            return 0;
        return Math.Round((decimal)((double)(after - before) / before * 100.0), 1);
    }

    public static string PercentChangeStr(this long before, long after) => PercentChange(before, after).ToString("F1") + " %";
}