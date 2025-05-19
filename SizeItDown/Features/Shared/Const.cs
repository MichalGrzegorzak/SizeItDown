namespace SizeItDown.Helpers;

public static class Const
{
    public static string[] ImagesExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".tiff"];
    
    public static string[] VideoExtensions = [".mp4", ".avi", ".mpg", ".mpeg", ".wmv", ".webm", ".mov", ".flv", ".mp"];

    public static string[] AllExtensions => VideoExtensions.Union(ImagesExtensions).ToArray();
}