using System.Text.Json;
using System.Linq;

// in order to be valid, a textbox must:
// 1. Not be deprecated -> chapter based
// 2. Not be unused -> chapter based
// 3. Not be empty in at least one language -> look at both languages

var langFolder = Path.Combine(Path.GetDirectoryName(FilePath), "lang");

var langList = Path.Combine(Path.GetDirectoryName(ScriptPath), "LangList");

OutputValid("ch1", "_ch1");
OutputValid("ch2", "");

void OutputValid (string chapterString, string langChapterString)
{
    var langJP = GetJson(Path.Combine(langFolder, "lang_ja" + langChapterString + ".json"));
    var langEN = GetJson(Path.Combine(langFolder, "lang_en" + langChapterString + ".json"));
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