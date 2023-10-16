#load "..\Lib\DecompileContext.csx"
#load "..\Lib\JsonUtils.csx"
#load "LangFile.csx"
#load "DeltarunePaths.csx"

using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Text.Encodings.Web;

async Task ExportCh2EnData ()
{

    var langJP = GetDeltaruneLangFile(Chapter.Chapter2, Lang.JP);
    var langEN = new ConcurrentDictionary<string, string>();

    // just to order the language back to the original order
    var output = new OrderedDictionary();
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

    string jsonString = JsonSerializer.Serialize(output, new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });

    ExportJson(output.ToDictionary(), Path.Combine(langFolder, "lang_en.json"));
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
    var codeContent = Decompiler.Decompile(code, DECOMPILE_CONTEXT.Value);
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