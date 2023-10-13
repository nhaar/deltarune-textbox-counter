using System.Text.Json;
using System.Linq;

EnsureDataLoaded();

var langFolder = Path.Combine(Path.GetDirectoryName(FilePath), "lang");

void ExportEmpty (Lang lang, Chapter chapter)
{
    var langName = lang switch
    {
        Lang.EN => "en",
        Lang.JP => "ja",
    };

    var chFileName = chapter switch
    {
        Chapter.Chapter1 => "_ch1",
        Chapter.Chapter2 => "",
    };

    var chName = chapter switch
    {
        Chapter.Chapter1 => "ch1",
        Chapter.Chapter2 => "ch2",
    };

    var langJSON = GetLang(langName + chFileName);

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

foreach (Lang lang in Enum.GetValues(typeof(Lang)))
{
    foreach (Chapter chapter in Enum.GetValues(typeof(Chapter)))
    {
        ExportEmpty(lang, chapter);
    }
}

Dictionary<string, string> GetLang (string langName)
{
    ReadOnlySpan<byte> fileBytes = File.ReadAllBytes(Path.Combine(langFolder, $"lang_{langName}.json"));
    var reader = new Utf8JsonReader(fileBytes);
    Dictionary<string, string> lang = new();

    var lastProperty = "";
    while (reader.Read())
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.PropertyName:
                lastProperty = reader.GetString();
                break;
            case JsonTokenType.String:
                lang[lastProperty] = reader.GetString();
                break;
        }
    }
    return lang;
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