#load "..\Lib\GetJson.csx"
#load "DeltaruneConstants.csx"
#load "DeltarunePaths.csx"

Dictionary<string, string> GetDeltaruneLangFile (Chapter chapter, Lang lang)
{
    return GetJsonAsDict(Path.Combine(langFolder, $"lang_{GetLangName(lang)}{GetLangFileName(chapter)}.json"));
}
