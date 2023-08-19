using System.Diagnostics;

namespace OxceToolsTests;

class SaveDir
{
    public string SaveFilePath { get; }
    public string ModifiedSaveFilePath { get; }
    public string ModifiedSaveFilePath2 { get; }

    internal SaveDir()
    {
        string homeDrive = Environment.GetEnvironmentVariable("HOMEDRIVE")!;
        string homePath = Environment.GetEnvironmentVariable("HOMEPATH")!;
        string saveDir = $"{homeDrive}{homePath}/OneDrive/Documents/OpenXcom/x-com-files";

        Debug.Assert(Directory.Exists(saveDir), $"saveDir: {saveDir}");
        
        SaveFilePath = Path.GetFullPath($"{saveDir}/toprocess.sav");
        ModifiedSaveFilePath = Path.GetFullPath($"{saveDir}/toprocess_modified.sav");
        ModifiedSaveFilePath2 = Path.GetFullPath($"{saveDir}/toprocess_modified2.sav");
    }
}