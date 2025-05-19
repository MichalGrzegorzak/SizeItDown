namespace SizeItDown.Generators;

public class MyStringBuilder
{
    private readonly string _filePath;
    private StringBuilder _sb = new StringBuilder();

    public MyStringBuilder()
    {
    }

    public MyStringBuilder(string filePath)
    {
        _filePath = filePath;
    }

    public MyStringBuilder AppendLine(string line, bool toConsole = false)
    {
        _sb.AppendLine(line);
        if(toConsole)
            Console.WriteLine(line);
        return this;
    }

    public MyStringBuilder Clear()
    {
        _sb.Clear();
        return this;
    }

    public MyStringBuilder WriteConsole()
    {
        Console.WriteLine(_sb.ToString());
        return this;
    }

    public MyStringBuilder AppendLineAndConsole(string line, bool toConsole = false) => AppendLine(line, true);
    
    public MyStringBuilder AppendToFile(string filePath = null)
    {
        File.AppendAllText(filePath ?? _filePath, _sb.ToString());
        _sb.Clear();
        return this;
    }

    public MyStringBuilder SaveToFile(string filePath = null)
    {
        File.WriteAllText(filePath ?? _filePath, _sb.ToString());
        _sb.Clear();
        return this;
    }

    public override string ToString()
    {
        return _sb.ToString();
    }
}