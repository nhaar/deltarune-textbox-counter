#load "DeltarunePaths.csx"
#load "DeltaruneConstants.csx"
#load "GetLang.csx"
#load "..\Lib\TextList.csx"

using System.Linq;

void GetAllRemaining ()
{
    foreach (Chapter chapter in Enum.GetValues(typeof(Chapter)))
    {
        var chapterString = GetChapterFileName(chapter);
        var langChapterString = GetLangFileName(chapter);
        try
        {
            GetRemaining(chapterString, langChapterString);
        }
        catch (System.Exception)
        {
            Console.WriteLine($"Error on {chapterString}");
        }
    }
}

void GetRemaining (string chapterString, string langChapterString)
{
    var langJP = GetLang("ja" + langChapterString);
    var langEN = GetLang("en" + langChapterString);
    var valid = GetFileTextList(Path.Combine(langFolder, "valid_" + chapterString + ".txt"));
    var test = GetFileTextList(Path.Combine(langList, "test-" + chapterString + ".txt"));

    var remaining = valid.Where(key => !test.Contains(key + $"_{chapterString}" )).ToList();
    remaining = remaining.Select(key => key + " //" + (langEN.ContainsKey(key) ? langEN[key] : langJP[key])).ToList();
    File.WriteAllLines(Path.Combine(langList, "remaining-" + chapterString + ".txt"), remaining);
}
