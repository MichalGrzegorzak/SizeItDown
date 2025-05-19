namespace SizeItDown.Generators;

public class MyContext
{
    // Static readonly instance ensures thread-safety and lazy initialization
    private static readonly Lazy<MyContext> _instance = new(() => new MyContext());

    // Private constructor to prevent external instantiation
    private MyContext()
    {
    }

    // Accessor for the singleton instance
    public static MyContext Instance => _instance.Value;

    // Your property
    public bool IsTestMode { get; set; }
}