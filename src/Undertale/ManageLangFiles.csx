#load "ExportLang.csx"
#load "..\Lib\JsonExclusive.csx"
#load "..\Lib\GetJson.csx"
#load "EmptyText.csx"

using System.Linq;


ExportLang();

var langEN = GetJsonAsDict(Path.Combine(langFolder, "lang_en.json"));
var langJP = GetJsonAsDict(Path.Combine(langFolder, "lang_ja.json"));

GetLanguageExclusive();
GetAllEmpty();
GetAllValid();
GetAllRemaining();

void GetLanguageExclusive ()
{
    var exclusive = GetJsonExclusive(langEN, langJP);
    File.WriteAllLines(Path.Combine(langFolder, "only_en.txt"), exclusive[0]);
    File.WriteAllLines(Path.Combine(langFolder, "only_ja.txt"), exclusive[1]);
}

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