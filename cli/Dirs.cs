using System.Diagnostics;

namespace OxceTools;

public class Dirs
{
    public Dirs()
    {
        string homeDrive = Environment.GetEnvironmentVariable("HOMEDRIVE")!;
        string homePath = Environment.GetEnvironmentVariable("HOMEPATH")!;
        string openXcomPath = $"{homeDrive}{homePath}/OneDrive/Documents/OpenXcom";
        string outputDir = $"{openXcomPath}";
        string saveDir = $"{openXcomPath}/x-com-files";
        string xcfPath = $"{openXcomPath}/mods/XComFiles";

        Debug.Assert(Directory.Exists(saveDir), $"saveDir: {saveDir}");
        
        SaveFilePath = Path.GetFullPath($"{saveDir}/toprocess.sav");
        ModifiedSaveFilePath = Path.GetFullPath($"{saveDir}/toprocess_MODIFIED.sav");
        MissionDataCsvFilePath = Path.GetFullPath($"{outputDir}/mission_data.csv");
        ModifiedMissionDataCsvFilePath = Path.GetFullPath($"{outputDir}/mission_data_MODIFIED.csv");
        MissionScriptsDataCsvFilePath = Path.GetFullPath($"{outputDir}/mission_scripts_data.csv");
        MissionScriptsPath = $"{xcfPath}/Ruleset/missionScripts_XCOMFILES_orig.rul";
    }

    public string SaveFilePath { get; }
    public string ModifiedSaveFilePath { get; }
    public string MissionDataCsvFilePath { get; }
    public string ModifiedMissionDataCsvFilePath { get; }
    public string MissionScriptsDataCsvFilePath { get; }
    public string MissionScriptsPath { get; }
}