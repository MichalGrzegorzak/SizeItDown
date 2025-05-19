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

    [Option('a', "autoReplace", Required = false, HelpText = "WARN - Still uses temp folder, but then will replace original files")]
    public bool AutoReplace { get; set; }
    
    [Option('v', "doVideos", Required = false, HelpText = "Do we convert images")]
    public bool DoVideos { get; set; } = true;

    [Option('g', "doImages", Required = false, HelpText = "Do we convert videos")]
    public bool DoImages { get; set; } = true;

    
    [Option('r', "manualReplace", Required = false, HelpText = "Call app again to replace original files, gives you safety and control")]
    public bool ManualReplace { get; set; }
    
    [Option('t', "test", Required = false, HelpText = "Will calculate, saved files in temp, but will NOT replace files from output to input")]
    public bool TestMode { get; set; }

    [Option('m', "motionFiles", Required = false, HelpText = "Will rename MP files to .mp4, and delete .jpg motion file")]
    public bool CleanMotionFiles { get; set; } = false;

    [Option('o', "tempOutputDir", Required = false, HelpText = "Temporary Output dir name")]
    public string TempOutDir { get; set; } = @"D:\Conversion\TEMP";

    [Option('p', "VideoPreset", Required = false, HelpText = "HandBrake Video Preset name")]
    public string VideoPreset { get; set; } = "HandbrakeVideoPreset.json";
    
    [Option('l', "list", Required = false, HelpText = "Just list files")]
    public bool List { get; set; }

    [Option('q', "ImageQuality", Required = false, HelpText = "Webp Image Quality, def: 80")]
    public int ImageQuality { get; set; } = 80;
    
    [Option('c', "ImageCropTo", Required = false, HelpText = "Max Image Width, will crop larger to it, def: 2560")]
    public int ImageCropTo { get; set; } = 2560;
}