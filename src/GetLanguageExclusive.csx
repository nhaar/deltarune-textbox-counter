using System.Text.Json;

var langFolder = Path.Combine(Path.GetDirectoryName(FilePath), "lang");

var jpCh1 = GetLang("ja_ch1");
var jpCh2 = GetLang("ja");
var enCh1 = GetLang("en_ch1");
var enCh2 = GetLang("en");

var ch1EnOnly = new HashSet<string>();
var ch2EnOnly = new HashSet<string>();
var ch1JpOnly = new HashSet<string>();
var ch2JpOnly = new HashSet<string>();

foreach (string textId in enCh1.Keys)
{
    if (!jpCh1.ContainsKey(textId))
    {
        ch1EnOnly.Add(textId + " //" + enCh1[textId]);
    }
}

foreach (string textId in enCh2.Keys)
{
    if (!jpCh2.ContainsKey(textId))
    {
        ch2EnOnly.Add(textId + " //" + enCh2[textId]);
    }
}

foreach (string textId in jpCh1.Keys)
{
    if (!enCh1.ContainsKey(textId))
    {
        ch1JpOnly.Add(textId + " //" + jpCh1[textId]);
    }
}

foreach (string textId in jpCh2.Keys)
{
    if (!enCh2.ContainsKey(textId))
    {
        ch2JpOnly.Add(textId + " //" + jpCh2[textId]);
    }
}

File.WriteAllLines(Path.Combine(langFolder, "only_jp_ch1.txt"), ch1EnOnly);
File.WriteAllLines(Path.Combine(langFolder, "only_en_ch1.txt"), ch1JpOnly);
File.WriteAllLines(Path.Combine(langFolder, "only_en_ch2.txt"), ch2EnOnly);
File.WriteAllLines(Path.Combine(langFolder, "only_jp_ch2.txt"), ch2JpOnly);

Dictionary<string, string> GetLang(string langName)
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