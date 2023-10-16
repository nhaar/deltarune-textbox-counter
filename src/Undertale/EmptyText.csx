#load "..\Lib\GetJson.csx"

void GetAllEmpty ()
{
    var langEN = GetJsonAsDict(Path.Combine(langFolder, "lang_en.json"));
    var langJP = GetJsonAsDict(Path.Combine(langFolder, "lang_ja.json"));

    var emptyEN = new List<string>();
    var emptyJP = new List<string>();

    foreach (string key in langEN.Keys)
    {
        if (string.IsNullOrWhiteSpace(langEN[key]))
        {
            emptyEN.Add(key);
        }
    }
    foreach (string key in langJP.Keys)
    {
        if (string.IsNullOrWhiteSpace(langJP[key]))
        {
            emptyJP.Add(key);
        }
    }

    File.WriteAllLines(Path.Combine(langFolder, "empty_en.txt"), emptyEN);
    File.WriteAllLines(Path.Combine(langFolder, "empty_ja.txt"), emptyJP);
}