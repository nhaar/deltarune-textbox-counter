#load "DeltarunePaths.csx"
#load "DeltaruneConstants.csx"
#load "..\Lib\TextList.csx"
#load "LangFile.csx"

using System.Linq;

void GetAllRemaining ()
{
    foreach (Chapter chapter in Enum.GetValues(typeof(Chapter)))
    {
        var chapterString = GetChapterFileName(chapter);
        var langChapterString = GetLangFileName(chapter);
        try
        {
            GetRemaining(chapter);
        }
        catch (System.Exception)
        {
            Console.WriteLine($"Error on {chapterString}");
        }
    }
}

void GetRemaining (Chapter chapter)
{
    var chapterString = GetChapterFileName(chapter);
    var langJP = GetDeltaruneLangFile(chapter, Lang.JP);
    var langEN = GetDeltaruneLangFile(chapter, Lang.EN);
    var valid = GetFileTextList(Path.Combine(langFolder, "valid_" + chapterString + ".txt"));
    var test = GetFileTextList(Path.Combine(langList, "test-" + chapterString + ".txt"));

    var remaining = valid.Where(key => !test.Contains(key + $"_{chapterString}" )).ToList();
    remaining = remaining.Select(key => key + " //" + (langEN.ContainsKey(key) ? langEN[key] : langJP[key])).ToList();
    File.WriteAllLines(Path.Combine(langList, "remaining-" + chapterString + ".txt"), remaining);
}
