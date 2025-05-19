using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace SizeItDown.Helpers;

public class Options
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Options))] //for AOT
    public Options()
    {
    }
    
    [Option('i', "inputDir", Required = true, HelpText = "Input dir name")]
    public string InputDir { get; set; }
    

    
    [Option('r', "replace", Required = false, HelpText = "Will replace files from output to input")]
    public bool Replace { get; set; }
    
    [Option('t', "test", Required = false, HelpText = "Will calculate, but will NOT replace files from output to input")]
    public bool TestMode { get; set; }

    [Option('m', "motionFiles", Required = false, HelpText = "Will rename MP files to .mp4, and delete .jpg motion file")]
    public bool CleanMotionFiles { get; set; } = false;

    [Option('o', "outputDir", Required = false, HelpText = "Temporary Output dir name")]
    public string OutputDir { get; set; } = @"D:\Conversion\TEMP";

    [Option('p', "VideoPreset", Required = false, HelpText = "HandBrake Video Preset name")]
    public string VideoPreset { get; set; } = "myPreset.json";
    
    [Option('l', "list", Required = false, HelpText = "Just list files")]
    public bool List { get; set; }

    [Option('q', "ImageQuality", Required = false, HelpText = "Webp Image Quality, def: 80")]
    public int ImageQuality { get; set; } = 80;
    
    [Option('c', "ImageCropTo", Required = false, HelpText = "Max Image Width, will crop larger to it, def: 2560")]
    public int ImageCropTo { get; set; } = 2560;
}