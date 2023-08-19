using System.Diagnostics;

namespace OxceTools;

public class SaveDir
{
    public string SaveFilePath { get; }
    public string ModifiedSaveFilePath { get; }

    public SaveDir()
    {
        string homeDrive = Environment.GetEnvironmentVariable("HOMEDRIVE")!;
        string homePath = Environment.GetEnvironmentVariable("HOMEPATH")!;
        string saveDir = $"{homeDrive}{homePath}/OneDrive/Documents/OpenXcom/x-com-files";

        Debug.Assert(Directory.Exists(saveDir), $"saveDir: {saveDir}");
        
        SaveFilePath = Path.GetFullPath($"{saveDir}/toprocess.sav");
        ModifiedSaveFilePath = Path.GetFullPath($"{saveDir}/toprocess_MODIFIED.sav");
    }
}