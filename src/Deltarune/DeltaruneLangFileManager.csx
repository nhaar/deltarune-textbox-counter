#load "DeltarunePaths.csx"
#load "ExportEnLang.csx"
#load "ExportCh1Deprecated.csx"
#load "GetLanguageExclusive.csx"
#load "GetEmptyText.csx"
#load "GenerateValidList.csx"
#load "GetRemaining.csx"

EnsureDataLoaded();

if (!File.Exists(Path.Combine(langFolder, "lang_en.json")))
{
    await ExportEnData();
}
if (!File.Exists(Path.Combine(langFolder, "deprecated_ch1.txt")))
{
    await ExportDeltaruneDeprecated();
}
if (!File.Exists(Path.Combine(langFolder, "only_en_ch1.txt")))
{
    GetLanguageExclusive();
}

GetAllEmpty();
GetAllValid();
GetAllRemaining();