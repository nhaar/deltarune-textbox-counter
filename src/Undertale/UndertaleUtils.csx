#load "..\Lib\JsonUtils.csx"

var langFolder = Path.Combine(Path.GetDirectoryName(FilePath), "lang");
var langList = Path.Combine(Path.GetDirectoryName(FilePath), "LangList");

enum Lang
{
    EN,
    JP
}

string GetUndertaleLangName (Lang lang)
{
    return lang switch 
    {
        Lang.EN => "en",
        Lang.JP => "ja",
    };
}

Dictionary<string, string> GetUndertaleLang (Lang lang)
{
    var langName = GetUndertaleLangName(lang);
    var langFile = Path.Combine(langFolder, "lang_" + langName + ".json");
    return GetJsonAsDict(langFile);
}