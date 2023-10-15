#load "..\Lib\JsonExclusive.csx"
#load "..\Lib\AddCommentToId.csx"
#load "DeltarunePaths.csx"
#load "DeltaruneConstants.csx"
#load "LangFile.csx"

using System.Linq;

void GetLanguageExclusive()
{
    foreach (Chapter chapter in Enum.GetValues(typeof(Chapter)))
    {
        GetChapterLanguageExclusive(chapter);
    }
}

void GetChapterLanguageExclusive (Chapter chapter)
{
    var langs = new List<Dictionary<string, string>>();
    var langTypes = (Lang[])Enum.GetValues(typeof(Lang));
    foreach (Lang lang in langTypes)
    {
        langs.Add(GetDeltaruneLangFile(chapter, lang));
    }

    var sets = GetJsonExclusive(langs.ToArray());

    for (int i = 0; i < langTypes.Length; i++)
    {   
        var langName = GetLangName(langTypes[i]);
        File.WriteAllLines
        (
            Path.Combine(langFolder, $"only_{langName}_{GetChapterFileName(chapter)}.txt"),
            sets[i].Select(textId => AddCommentToId(textId, langs.ToArray()))            
        );
    }
}