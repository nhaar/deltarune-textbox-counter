// This script setups the language files for first time use

#load "..\Lib\DecompileContext.csx"
#load "..\Lib\JsonUtils.csx"
#load "..\Lib\TextIdList.csx"
#load "DeltaruneUtils.csx"
#load "..\Lib\Deprecated.csx"

using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Linq;

EnsureDataLoaded();
await ExportCh2EnData();
await ExportDeltaruneDeprecated();
GetLanguageExclusive();

/// <summary>
/// Export deltarune deprecated text ids
/// </summary>
/// <returns></returns>
async Task ExportDeltaruneDeprecated ()
{
    foreach (Chapter chapter in Enum.GetValues(typeof(Chapter)))
    {
        bool condition (string name)
        {
            switch (chapter)
            {
                case Chapter.Chapter1:
                    return name.Contains("ch1");
                case Chapter.Chapter2:
                    return !name.Contains("ch1");
                default:
                    return false;
            }
        }
        List<UndertaleCode> codeList = Data.Code.Where(code => condition(code.Name.Content) && code.ParentEntry == null).ToList();
        var langEN = GetDeltaruneLangFile(chapter, Lang.EN);
        var langJP = GetDeltaruneLangFile(chapter, Lang.JP);
        var deprecated = await GetDeprecated(codeList.ToArray(), langEN, langJP);

        File.WriteAllLines
        (
            Path.Combine(langFolder, $"deprecated_{GetChapterFileName(chapter)}.txt"),
            deprecated.Select(textId => AddCommentToId(textId, langEN, langJP))
        );
    }
}

/// <summary>
/// Export the chapter 2 english language file as a json
/// </summary>
/// <returns></returns>
async Task ExportCh2EnData ()
{

    var langJP = GetDeltaruneLangFile(Chapter.Chapter2, Lang.JP);
    var langEN = new ConcurrentDictionary<string, string>();

    // just to order the language back to the original order
    var output = new Dictionary<string, string>();
    // code entries with ch1 are the ones that are going to be present in the chapter 1 lang file
    // although there ARE chapter 1 strings in the chapter 2 lang file, they are not used
    // don't know why ParentEntry needs to be null, but that's how it was in the ExportAllCode.csx script
    List<UndertaleCode> ch2Code = Data.Code.Where(code => !code.Name.Content.Contains("ch1") && code.ParentEntry == null).ToList();

    SetProgressBar(null, "Extracting Chapter 2 Text", 0, ch2Code.Count);
    StartProgressBarUpdater();
    while (true)
    {
        try
        {
            await Parallel.ForEachAsync(ch2Code, async (code, cancellationToken) => SearchInCode(code, langJP, langEN));
            break;
        }
        catch (System.Exception)
        {            
        }

    }
    await StopProgressBarUpdater();

    // the legendary one pipis textbox that has a pagebreak (bug)
    langEN["obj_pipis_enemy_slash_Step_0_gml_97_0"] = langEN["obj_pipis_enemy_slash_Step_0_gml_97_0"].TrimStart('\f');

    foreach (string textCode in langJP.Keys)
    {
        if (langEN.ContainsKey(textCode))
        {
            output[textCode] = langEN[textCode];
        }
    }

    ExportJson(output, Path.Combine(langFolder, "lang_en.json"));
}


/// <summary>
/// Get the number of possible text codes in a line of GML code
/// 
/// It is possible for it to return a number higher than the actual number of text codes in the line,
/// but it will never return a number lower than the actual number of text codes in the line
/// 
/// It is used to know if we can skip a line when searching for text codes
/// </summary>
/// <param name="line"></param>
/// <returns></returns>
int GetPossibleTextCodes (string line)
{
    return Regex.Matches(line, @"\(.*?""[\w\d]+"".*?\)").Count;
}


/// <summary>
/// Get the first STRING argument of a function call
/// </summary>
/// <param name="functionCall"></param>
/// <returns></returns>
/// <remarks>
/// This function is used to get the language string from a function call, which is always
/// the first string argument of the function
/// </remarks>
string GetFirstArgument (string functionCall)
{
    return Regex.Match(functionCall, @"(?<=^\([^""]*"")(\\""|[^""])*(?="")").Value;
}

/// <summary>
/// Search inside a code entry for all the text codes and their corresponding language strings
/// </summary>
/// <param name="code"></param>
void SearchInCode (UndertaleCode code, Dictionary<string, string> langJP, ConcurrentDictionary<string, string> langEN)
{
    var codeContent = Decompile(code);
    var codeLines = codeContent.Split("\n").ToArray();

    // keeping track of the possible so that we will know when we can skip a line for certain
    var possibleCodeCount = codeLines.Select(GetPossibleTextCodes).ToArray();

    for (int i = 0; i < codeLines.Length; i++)
    {
        var line = codeLines[i];
        foreach (string textCode in langJP.Keys)
        {
            // do it in this loop since it will be decremented in here
            if (possibleCodeCount[i] == 0)
            {
                break;
            }
            if (langEN.ContainsKey(textCode))
            {
                continue;
            }
            if (line.Contains($"\"{textCode}\""))
            {
                string langString = null;
                var functions = new[]
                {
                    "msgnextloc",
                    "stringsetsubloc",
                    "stringsetloc",
                    "msgsetloc",
                    "c_msgnextloc",
                    "c_msgsetloc",
                    "msgnextsubloc",
                    "msgsetsubloc"
                };

                for (int j = 0; j < functions.Length; j++)
                {
                    Match match = Regex.Match(line, @$"\b{functions[j]}\(.*?""{textCode}""\)");
                    if (match.Success)
                    {
                        var split = match.Value.Split(functions[j]);
                        foreach (string s in split)
                        {
                            if (s.Contains(textCode))
                            {
                                langString = GetFirstArgument(s);
                                break;
                            }
                        }
                        break;
                    }
                }

                // if the language string is null, it means that we couldn't find the function call
                // so the it was not an actual text code, but a variable or something else
                if (langString != null)
                {
                    possibleCodeCount[i]--;
                    langEN[textCode] = Regex.Unescape(langString);
                }
            }
        }
    }

    IncrementProgressParallel();
}

/// <summary>
/// Get the text ids that are exclusive to each language
/// </summary>
void GetLanguageExclusive()
{
    foreach (Chapter chapter in Enum.GetValues(typeof(Chapter)))
    {
        GetChapterLanguageExclusive(chapter);
    }
}

/// <summary>
/// Get the text ids that are exclusive to each language in a chapter
/// </summary>
/// <param name="chapter"></param>
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