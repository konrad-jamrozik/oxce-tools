using System.Diagnostics;

namespace OxceTools;

public class Dirs
{
    public string SaveFilePath { get; }
    public string ModifiedSaveFilePath { get; }
    public string OutputCsvFilePath { get; }

    public Dirs()
    {
        string homeDrive = Environment.GetEnvironmentVariable("HOMEDRIVE")!;
        string homePath = Environment.GetEnvironmentVariable("HOMEPATH")!;
        string saveDir = $"{homeDrive}{homePath}/OneDrive/Documents/OpenXcom/x-com-files";
        string outputDir = $"{homeDrive}{homePath}/OneDrive/Documents/OpenXcom/";

        Debug.Assert(Directory.Exists(saveDir), $"saveDir: {saveDir}");
        
        SaveFilePath = Path.GetFullPath($"{saveDir}/toprocess.sav");
        ModifiedSaveFilePath = Path.GetFullPath($"{saveDir}/toprocess_MODIFIED.sav");
        OutputCsvFilePath = Path.GetFullPath($"{outputDir}/data_from_save_file.csv");
    }
}