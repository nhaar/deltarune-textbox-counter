// This script updates the lang files with the latest changes

#load "ExportLang.csx"
#load "..\Lib\GetJson.csx"
#load "EmptyText.csx"

using System.Linq;

var langEN = GetJsonAsDict(Path.Combine(langFolder, "lang_en.json"));
var langJP = GetJsonAsDict(Path.Combine(langFolder, "lang_ja.json"));

GetAllEmpty();
GetAllValid();
GetAllRemaining();

void GetAllValid ()
{
    var emptyEN = File.ReadAllLines(Path.Combine(langFolder, "empty_en.txt"));
    var emptyJP = File.ReadAllLines(Path.Combine(langFolder, "empty_ja.txt"));
    var empty = new HashSet<string>();

    foreach (string id in emptyEN)
    {
        if (emptyJP.Contains(id))
        {
            empty.Add(id);
        }
    }

    var unused = File.ReadAllLines(Path.Combine(langFolder, "unused.txt"));

    var valid = langEN.Keys.Union(langJP.Keys).Except(empty).Except(unused);

    File.WriteAllLines(Path.Combine(langFolder, "valid.txt"), valid);
}

void GetAllRemaining ()
{
    var valid = File.ReadAllLines(Path.Combine(langFolder, "valid.txt"));
    var test = File.ReadAllLines(Path.Combine(langFolder, "test.txt"));

    var remaining = valid.Except(test);
    File.WriteAllLines(Path.Combine(langFolder, "remaining.txt"), remaining);
}


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
}