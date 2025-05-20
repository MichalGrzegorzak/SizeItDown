using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace SizeItDown.Helpers;

public class Options
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Options))] //for AOT
    public Options()
    {
    }
    
    [Option('a', "autoReplace", Required = false, HelpText = "WARN - Still uses temp folder, but then will replace original files")]
    public bool AutoReplace { get; set; }
    
    [Option('v', "doVideos", Required = false, HelpText = "Do we convert images")]
    public bool DoVideos { get; set; }

    [Option('g', "doImages", Required = false, HelpText = "Do we convert videos")]
    public bool DoImages { get; set; }
    
    [Option('r', "replaceMode", Required = false, HelpText = "Call app second time to replace files in TempOutDir, gives you safety and control")]
    public bool ReplaceMode { get; set; }
    
    [Option('t', "test", Required = false, HelpText = "Will calculate, saved files in temp, but will NOT replace files from output to input")]
    public bool TestMode { get; set; }
    
    [Option('l', "list", Required = false, HelpText = "Just list files")]
    public bool ListMode { get; set; }
    
    [Option('m', "motionFiles", Required = false, HelpText = "Will rename MP files to .mp4, and delete .jpg motion file")]
    public bool CleanMotionFiles { get; set; } = false;
    
    [Option('i', "inputDir", Required = true, HelpText = "Input dir name")]
    public string InputDir { get; set; }

    [Option('o', "tempOutputDir", Required = false, HelpText = "Temporary Output dir name")]
    public string TempOutDir { get; set; } = @"D:\Conversion\TEMP";

    [Option('p', "videoPreset", Required = false, HelpText = "HandBrake Video Preset name")]
    public string VideoPreset { get; set; } = "HandbrakeVideoPreset.json";
    
    [Option('q', "imageQuality", Required = false, HelpText = "Image Quality, def: 80")]
    public int ImageQuality { get; set; } = 80;
    
    [Option('w', "imageMaxWidth", Required = false, HelpText = "Max Image Width, will crop larger to it, def: 2560")]
    public int ImageMaxWidth { get; set; } = 2560;
    
    [Option('c', "imageConvTo", Required = false, HelpText = "Converts to WebP or Avif")]
    public string ImageConvTo { get; set; } = "WebP";
    
    [Option('d', "takeOnlyOlderFilesByXdays", Required = false, HelpText = "Additional protection, to not process same files twice")]
    public int FilterFilesOlderThanXdays { get; set; } = 1;
}