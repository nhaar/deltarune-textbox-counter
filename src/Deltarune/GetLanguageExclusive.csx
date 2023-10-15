#load "DecompileContext.csx"
#load "DeltarunePaths.csx"
#load "DeltaruneConstants.csx"
#load "GetLang.csx"

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
    var langs = new Dictionary<string, Dictionary<string, string>>();
    foreach (Lang lang in Enum.GetValues(typeof(Lang)))
    {
        langs[GetLangName(lang)] = GetLang(GetLangName(lang) + GetLangFileName(chapter));
    }

    foreach (string lang in langs.Keys)
    {   
        var thisLang = langs[lang];
        var languageOnly = new HashSet<string>();
        var otherLang = langs.Where(l => l.Key != lang).First().Key;
        foreach (string textId in thisLang.Keys)
        {
            if (!langs[otherLang].ContainsKey(textId))
            {
                languageOnly.Add(textId + " //" + thisLang[textId]);
            }
        }

        File.WriteAllLines(Path.Combine(langFolder, $"only_{lang}_{GetChapterFileName(chapter)}.txt"), languageOnly);
    }
}