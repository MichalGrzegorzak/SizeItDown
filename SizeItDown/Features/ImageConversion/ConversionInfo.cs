namespace SizeItDown.Generators;

public class ConversionInfo
{
    public string FileName { get; set; } 
    public long SizeBefore { get; set; } 
    public long SizeAfter { get; set; }
    
    public bool WasResized { get; set; }

    public decimal Reduction => (SizeBefore - SizeAfter).ToKB();
    public string Percentage => (SizeBefore.PercentChange(SizeAfter)*-1).ToString("F1") + " %";
}