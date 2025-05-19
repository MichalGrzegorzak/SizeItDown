namespace SizeItDown.Helpers;

public static class Const
{
    public static string[] ImagesExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".tiff"];
    
    public static string[] VideoExtensions = [".mp4", ".avi", ".mpg", ".mpeg", ".wmv", ".webm", ".flv", ".mp"];
    //".mov" - 265 produces larger files

    public static string[] AllExtensions => VideoExtensions.Union(ImagesExtensions).ToArray();
}