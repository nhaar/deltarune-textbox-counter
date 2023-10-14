using System.Linq;
using System.Text.Json;

EnsureDataLoaded();

var langFolder = Path.Combine(Path.GetDirectoryName(FilePath), "lang");

var langList = Path.Combine(Path.GetDirectoryName(ScriptPath), "LangList");

void GetValid (string chapterString, string langChapterString)
{
    var langJP = GetJson(Path.Combine(langFolder, "lang_ja" + langChapterString + ".json"));
    Console.WriteLine(langJP["date"]);
    var langEN = GetJson(Path.Combine(langFolder, "lang_en" + langChapterString + ".json"));
    var valid = GetFileTextList(Path.Combine(langFolder, "valid_" + chapterString + ".txt"));
    var test = GetFileTextList(Path.Combine(langList, "test-" + chapterString + ".txt"));

    var remaining = valid.Where(key => !test.Contains(key + $"_{chapterString}" )).ToList();
    remaining = remaining.Select(key => key + " //" + (langEN.ContainsKey(key) ? langEN[key] : langJP[key])).ToList();
    File.WriteAllLines(Path.Combine(langList, "remaining-" + chapterString + ".txt"), remaining);
}

GetValid("ch1", "_ch1");

Dictionary<string, string> GetJson (string filePath)
{
    ReadOnlySpan<byte> fileBytes = File.ReadAllBytes(filePath);
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

List<string> GetFileTextList (string filePath)
{
    var lines = File.ReadAllLines(filePath).ToList();
    var commentPattern = new Regex(@"(?<=^.*?)//.*$", RegexOptions.Multiline);
    lines = lines.Select(line => commentPattern.Replace(line, "")).ToList();
    lines = lines.Select(lines => lines.Trim()).ToList();
    lines.RemoveAll(line => line == "");
    return lines;
}