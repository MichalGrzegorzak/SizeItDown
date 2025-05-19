namespace SizeItDown.Helpers;

public record FileSizes
{
    const int oneKB = 1024;
    
    public FileSizes(FileInfo input, FileInfo output)
    {
        Name = input.Name;
        Directory = output.Directory.FullName;
        Input = input.Length / oneKB;
        Output = output.Length / oneKB;
        Diff = Input - Output;
    }
    
    public string Directory { get; }
    
    public string Name { get; }
    public long Input { get; }
    public long Output { get; }
    public long Diff { get; private set; }

    public void ResetDiff() => Diff = 0;

    public override string ToString()
    {
        return $"{Name} -> inp. size: {Input} KB, out. size: {Output} KB, diff: {Diff} KB";
    }
}