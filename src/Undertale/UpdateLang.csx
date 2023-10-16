// This script updates the lang files with the latest changes

#load "..\Lib\GetJson.csx"
#load "UndertaleUtils.csx"
#load "..\Lib\AddComment.csx"
#load "..\Lib\TextList.csx"

using System.Linq;

var langEN = GetUndertaleLang(Lang.EN);
var langJP = GetUndertaleLang(Lang.JP);

GetAllEmpty();
GetAllValid();
GetAllRemaining();

void GetAllValid ()
{
    var emptyEN = GetFileTextList(Path.Combine(langFolder, "empty_en.txt"));
    var emptyJP = GetFileTextList(Path.Combine(langFolder, "empty_ja.txt"));
    var empty = new HashSet<string>(emptyEN.Intersect(emptyJP));

    var unused = GetFileTextList(Path.Combine(langList, "unused.txt"));

    var valid = langEN.Keys.Union(langJP.Keys).Except(empty).Except(unused);

    WriteWithComments(Path.Combine(langFolder, "valid.txt"), valid.ToList(), langEN, langJP);
}

void GetAllRemaining ()
{
    var valid = GetFileTextList(Path.Combine(langFolder, "valid.txt"));
    var test = GetFileTextList(Path.Combine(langFolder, "test.txt"));

    var remaining = valid.Except(test);
    WriteWithComments(Path.Combine(langFolder, "remaining.txt"), remaining.ToList(), langEN, langJP);
}


void GetAllEmpty ()
{
    var emptyEN = FilterEmpty(langEN);
    var emptyJP = FilterEmpty(langJP);

    WriteWithComments(Path.Combine(langFolder, "empty_en.txt"), emptyEN, langEN, langJP);
    WriteWithComments(Path.Combine(langFolder, "empty_ja.txt"), emptyJP, langEN, langJP);
}

List<string> FilterEmpty (Dictionary<string, string> lang)
{
    return lang.Where(kv => string.IsNullOrWhiteSpace(kv.Value)).Select(kv => kv.Key).ToList();
}