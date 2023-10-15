#load "DeltarunePaths.csx"
#load "..\Lib\GetJson.csx"

Dictionary<string, string> GetLang(string langName)
{
    return GetJsonAsDict(Path.Combine(langFolder, $"lang_{langName}.json"));
}