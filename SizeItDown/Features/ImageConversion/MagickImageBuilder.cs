using ImageMagick;

namespace SizeItDown.Generators;

public class MagickImageBuilder : IDisposable
{
    private readonly MyStringBuilder _sb;
    private MagickImage _image;
    private string _ext;
    string _outputPath;
    private ConversionInfo _ci;
    private FileInfo _inpImg;
    private static readonly object _lock = new object();

    public MagickImageBuilder(string imagePath, MyStringBuilder sb)
    {
        _sb = sb;
        _image = new MagickImage(imagePath);
        _outputPath = imagePath;
        _inpImg = new FileInfo(imagePath);
        _ci = new ConversionInfo() { FileName = _inpImg.Name, SizeBefore = _inpImg.Length };
    }

    public MagickImageBuilder Resize(int maxWidth)
    {
        if (_image.Width <= maxWidth)
            return this;
        
        // Calculate new height to maintain aspect ratio
        int newHeight = (int)((double)maxWidth / _image.Width * _image.Height);
        
        lock (_lock)
        {
            _sb.AppendLineAndConsole($"Resizing from: {_image.Width}x{_image.Height} to {maxWidth}x{newHeight}.");
        }

        _image.Resize((uint)maxWidth, (uint)newHeight);
        _ci.WasResized = true;
        return this;
    }

    public MagickImageBuilder Convert(MagickFormat format, int quality = 80)
    {
        _image.Format = format;
        _image.Quality = (uint)quality;
        _outputPath = _outputPath.Replace(_inpImg.Extension, $".{_image.Format.ToString().ToLower()}");
        return this;
    }

    public ConversionInfo Save(string toPath = null, bool deleteOrg = true)
    {
        if (toPath != null)
            _outputPath = toPath;
        
        _image.Write(_outputPath);
        _ci.SizeAfter = new FileInfo(_outputPath).Length;

        if (MyAppContext.Instance.IsTestMode)
        {
            _image.Dispose();
            File.Delete(_outputPath);
        }
        else
        {
            if (deleteOrg)
                _inpImg.Delete();
        }
       
        return _ci;
    }

    public void Dispose()
    {
        _image?.Dispose();
    }
}