#load "DeltarunePaths.csx"
#load "DeltaruneConstants.csx"
#load "LangFile.csx"

using System.Linq;

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

    
    File.WriteAllLines(Path.Combine(langFolder, $"empty_{chName}_{langName}.txt"), empty);
}

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