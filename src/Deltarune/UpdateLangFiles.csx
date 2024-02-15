#load "DeltaruneUtils.csx"
#load "..\Lib\JsonUtils.csx"
#load "..\Lib\TextIdList.csx"

using System.Linq;

EnsureDataLoaded();
GetAllEmpty();
GetAllValid();
GetAllRemaining();

/// <summary>
/// Export all empty text ids
/// </summary>
void GetAllEmpty ()
{
    foreach (Lang lang in Enum.GetValues(typeof(Lang)))
    {
        foreach (Chapter chapter in Enum.GetValues(typeof(Chapter)))
        {
            ExportEmpty(lang, chapter);
        }
    }
}

/// <summary>
/// Export all empty text ids for a language and chapter
/// </summary>
/// <param name="lang"></param>
/// <param name="chapter"></param>
void ExportEmpty (Lang lang, Chapter chapter)
{
    var langName = GetLangName(lang);
    var chName = GetChapterFileName(chapter);
    var chFileName = GetLangFileName(chapter);
    var langJSON = GetDeltaruneLangFile(chapter, lang);

    List<string> empty = new();

    foreach (string key in langJSON.Keys)
    {
        var text = langJSON[key];
        text = RemoveSpecialCharacters(text).Trim();
        if (text == "")
            empty.Add(key);
    }

    WriteIdListWithComments(Path.Combine(langFolder, $"empty_{chName}_{langName}.txt"), empty, langJSON);
}

/// <summary>
/// Remove special control characters from a text
/// </summary>
/// <param name="text"></param>
/// <returns></returns>
string RemoveSpecialCharacters (string text)
{
    var patterns = new[]
    {
        @"\\M\d+",
        @"\\C\d+",
        @"\\E\d+",
        @"\\T\w",
        @"\\T\w+",
        @"\\F\d+",
        @"\\F\w",
        @"\\E\w",
        @"\\I\d+",
        @"\\c\w",
        @"\^\d+"
    };
    foreach (string pattern in patterns)
    {
        Regex regex = new Regex(pattern);
        text = regex.Replace(text, "");
    }
    return text;
}


// in order to be valid, a textbox must:
// 1. Not be deprecated -> chapter based
// 2. Not be unused -> chapter based
// 3. Not be empty in at least one language -> look at both languages

/// <summary>
/// Export all valid text ids
/// </summary>
void GetAllValid ()
{
    foreach (Chapter chapter in Enum.GetValues(typeof(Chapter)))
    {
        var chapterString = GetChapterFileName(chapter);
        var langChapterString = GetLangFileName(chapter);
        OutputValid(chapterString, langChapterString);
    }
}

/// <summary>
/// Export all valid text ids for a chapter
/// </summary>
/// <param name="chapterString"></param>
/// <param name="langChapterString"></param>
void OutputValid (string chapterString, string langChapterString)
{
    var langJP = GetJsonAsDict(Path.Combine(langFolder, "lang_ja" + langChapterString + ".json"));
    var langEN = GetJsonAsDict(Path.Combine(langFolder, "lang_en" + langChapterString + ".json"));
    var deprecated = GetTextIdList(Path.Combine(langFolder, "deprecated_" + chapterString + ".txt"));
    var unused = GetTextIdList(Path.Combine(langList, "unused-" + chapterString + ".txt"));
    var emptyJP = GetTextIdList(Path.Combine(langFolder, "empty_" + chapterString + "_ja.txt"));
    var emptyEN = GetTextIdList(Path.Combine(langFolder, "empty_" + chapterString + "_en.txt"));

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
            validKeys.Add(AddCommentToId(key, langEN, langJP));
        }
    }
    File.WriteAllLines(Path.Combine(langFolder, "valid_" + chapterString + ".txt"), validKeys);
}

/// <summary>
/// Export all remaining text ids
/// </summary>
void GetAllRemaining ()
{
    foreach (Chapter chapter in Enum.GetValues(typeof(Chapter)))
    {
        var chapterString = GetChapterFileName(chapter);
        var langChapterString = GetLangFileName(chapter);
        GetRemaining(chapter);
    }
}

/// <summary>
/// Export all remaining text ids for a chapter
/// </summary>
/// <param name="chapter"></param>
void GetRemaining (Chapter chapter)
{
    var chapterString = GetChapterFileName(chapter);
    var langJP = GetDeltaruneLangFile(chapter, Lang.JP);
    var langEN = GetDeltaruneLangFile(chapter, Lang.EN);
    var valid = GetTextIdList(Path.Combine(langFolder, "valid_" + chapterString + ".txt"));
    var test = GetTextIdList(Path.Combine(langFolder, "test-" + chapterString + ".txt"));

    var remaining = valid.Where(key => !test.Contains(key + $"_{chapterString}" )).ToList();
    remaining = remaining.Select(key => key + " //" + (langEN.ContainsKey(key) ? langEN[key] : langJP[key])).ToList();
    File.WriteAllLines(Path.Combine(langFolder, "remaining-" + chapterString + ".txt"), remaining);
}
