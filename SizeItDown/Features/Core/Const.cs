namespace SizeItDown.Helpers;

public static class Const
{
    public static string[] ImagesExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".tiff"];
    // you don't want to covert ".gif"
    
    public static string[] VideoExtensions = [".mp4", ".avi", ".mpg", ".mpeg", ".webm", ".flv", ".mp"];
    //LARGER after H265 compression: ".3gp", ".mov", ".wmv"

    public static string[] AllExtensions => VideoExtensions.Union(ImagesExtensions).ToArray();
}