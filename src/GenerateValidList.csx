using System.Text.Json;
using System.Linq;

var langFolder = Path.Combine(Path.GetDirectoryName(FilePath), "lang");

var langList = Path.Combine(Path.GetDirectoryName(ScriptPath), "LangList");

var ch1JP = GetJson(Path.Combine(langFolder, "lang_ja_ch1.json"));
var ch2JP = GetJson(Path.Combine(langFolder, "lang_ja.json"));
var ch1EN = GetJson(Path.Combine(langFolder, "lang_en_ch1.json"));
var ch2EN = GetJson(Path.Combine(langFolder, "lang_en.json"));
var deprecatedCh1 = GetFileTextList(Path.Combine(langFolder, "deprecated_ch1.txt"));
var deprecatedCh2 = GetFileTextList(Path.Combine(langFolder, "deprecated.txt"));
var unusedCh1 = GetFileTextList(Path.Combine(langList, "unused-ch1.txt"));
var unusedCh2 = GetFileTextList(Path.Combine(langList, "unused-ch2.txt"));

File.WriteAllLines(Path.Combine(langFolder, "valid_ch1.txt"), GetValidKeys(ch1JP, deprecatedCh1, unusedCh1, ch1EN));

File.WriteAllLines(Path.Combine(langFolder, "valid_ch2.txt"), GetValidKeys(ch2JP, deprecatedCh2, unusedCh2, ch2EN));

List<string> GetValidKeys (Dictionary<string, string> json, List<string> deprecated, List<string> unused, Dictionary<string, string> alt)
{
    var validKeys = new List<string>();
    foreach (string key in json.Keys)
    {
        if (!deprecated.Contains(key) && !unused.Contains(key))
        {
            var text = alt.ContainsKey(key) ? alt[key] : json[key];
            validKeys.Add(key + " //" + text);
        }
    }
    return validKeys;
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
    lines.Select(lines => lines.Trim());
    lines.RemoveAll(line => line == "");
    return lines;
}