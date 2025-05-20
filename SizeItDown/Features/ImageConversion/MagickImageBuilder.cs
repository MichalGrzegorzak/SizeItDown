using ImageMagick;

namespace SizeItDown.Generators;

public class MagickImageBuilder : IDisposable
{
    private readonly MyStringBuilder _sb;
    private MagickImage _image;
    private string _ext;
    private ConversionInfo _ci;
    private FileInfo _inpImg;
    private static readonly object _lock = new object();

    public MagickImageBuilder(string imagePath, MyStringBuilder sb)
    {
        _sb = sb;
        _image = new MagickImage(imagePath);
        _inpImg = new FileInfo(imagePath);
        _ext = _inpImg.Extension;
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
        _ext = $".{_image.Format.ToString().ToLower()}";
        return this;
    }

    public async Task<ConversionInfo> SaveAsync(string toPath, bool deleteOrg = true)
    {
        toPath = Path.ChangeExtension(toPath, _ext);
        //toPath = toPath.Replace(_inpImg.Extension, _ext); //changing extension if we converted
        
        await _image.WriteAsync(toPath);
        _ci.SizeAfter = new FileInfo(toPath).Length;

        if (deleteOrg)
            _inpImg.Delete();
        
        return _ci;
    }

    public void Dispose()
    {
        _image?.Dispose();
    }
}