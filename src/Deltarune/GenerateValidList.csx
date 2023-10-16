#load "..\Lib\JsonUtils.csx"
#load "..\Lib\TextList.csx"
#load "DeltarunePaths.csx"

using System.Linq;

// in order to be valid, a textbox must:
// 1. Not be deprecated -> chapter based
// 2. Not be unused -> chapter based
// 3. Not be empty in at least one language -> look at both languages

void GetAllValid ()
{
    foreach (Chapter chapter in Enum.GetValues(typeof(Chapter)))
    {
        var chapterString = GetChapterFileName(chapter);
        var langChapterString = GetLangFileName(chapter);
        OutputValid(chapterString, langChapterString);
    }
}

void OutputValid (string chapterString, string langChapterString)
{
    var langJP = GetJsonAsDict(Path.Combine(langFolder, "lang_ja" + langChapterString + ".json"));
    var langEN = GetJsonAsDict(Path.Combine(langFolder, "lang_en" + langChapterString + ".json"));
    var deprecated = GetFileTextList(Path.Combine(langFolder, "deprecated_" + chapterString + ".txt"));
    var unused = GetFileTextList(Path.Combine(langList, "unused-" + chapterString + ".txt"));
    var emptyJP = GetFileTextList(Path.Combine(langFolder, "empty_" + chapterString + "_ja.txt"));
    var emptyEN = GetFileTextList(Path.Combine(langFolder, "empty_" + chapterString + "_en.txt"));

    var allKeys = langEN.Concat(langJP.Where(kv => !langEN.ContainsKey(kv.Key))).ToDictionary(kv => kv.Key, kv => kv.Value).Keys.ToList();
    var validKeys = new HashSet<string>();
    foreach (string key in allKeys)
    {
        if
        (
            !deprecated.Contains(key) &&
            !unused.Contains(key) &&
            (!emptyJP.Contains(key) || !emptyEN.Contains(key))
        )
        {
            var text = langEN.ContainsKey(key) ? langEN[key] : langJP[key];
            validKeys.Add(key + " //" + text);
        }
    }
    File.WriteAllLines(Path.Combine(langFolder, "valid_" + chapterString + ".txt"), validKeys);
}