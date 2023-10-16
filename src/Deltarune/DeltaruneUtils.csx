#load "..\Lib\JsonUtils.csx"

using System.Runtime.Serialization;

enum Lang
{
    EN,
    JP
}

enum Chapter
{
    Chapter1,
    Chapter2
}

string GetLangName (Lang lang)
{
    return lang switch
    {
        Lang.EN => "en",
        Lang.JP => "ja",
    };
}

string GetLangFileName (Chapter chapter)
{
    return chapter switch
    {
        Chapter.Chapter1 => "_ch1",
        Chapter.Chapter2 => "",
    };
}

string GetChapterFileName (Chapter chapter)
{
    return chapter switch
    {
        Chapter.Chapter1 => "ch1",
        Chapter.Chapter2 => "ch2",
    };
}

var langFolder = Path.Combine(Path.GetDirectoryName(FilePath), "lang");

var langList = Path.Combine(Path.GetDirectoryName(ScriptPath), "LangList");

Dictionary<string, string> GetDeltaruneLangFile (Chapter chapter, Lang lang)
{
    return GetJsonAsDict(Path.Combine(langFolder, $"lang_{GetLangName(lang)}{GetLangFileName(chapter)}.json"));
}
