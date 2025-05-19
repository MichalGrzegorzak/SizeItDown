namespace SizeItDown.Generators;

public class MyAppContext
{
    // Static readonly instance ensures thread-safety and lazy initialization
    private static readonly Lazy<MyAppContext> _instance = new(() => new MyAppContext());

    // Private constructor to prevent external instantiation
    private MyAppContext()
    {
    }

    // Accessor for the singleton instance
    public static MyAppContext Instance => _instance.Value;

    // Your property
    public bool IsTestMode { get; set; }
}