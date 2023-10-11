using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using System.Text.Encodings.Web;

// TO-DO: Ensure DELTARUNE DEMO 1&2 is loaded as well
EnsureDataLoaded();

// needed for decompiling the code entries
ThreadLocal<GlobalDecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<GlobalDecompileContext>(() => new GlobalDecompileContext(Data, false));

var langFolder = Path.Combine(Path.GetDirectoryName(FilePath), "lang");

/// <summary>
/// Load the JSON for the Japanese language file as a dictionary and extract the keys
/// </summary>
/// <returns></returns>
// for some reason, the read only spans and utf8jsonreader only will work inside a function and not in the global scope
List<string> GetLangJP()
{
    ReadOnlySpan<byte> fileBytes = File.ReadAllBytes(Path.Combine(langFolder, "lang_ja.json"));
    var reader = new Utf8JsonReader(fileBytes);
    List<string> langJP = new();

    while (reader.Read())
    {
        // only need the keys
        if (reader.TokenType == JsonTokenType.PropertyName)
            {
            langJP.Add(reader.GetString());
        }
    }
    return langJP;
}

var langJP = GetLangJP();

var langEN = new ConcurrentDictionary<string, string>();

// just to order the language back to the original order
var output = new Dictionary<string, string>();

List<UndertaleCode> allCode = new();

foreach (UndertaleCode code in Data.Code)
{
    // code entries with ch1 are the ones that are going to be present in the chapter 1 lang file
    // although there ARE chapter 1 strings in the chapter 2 lang file, they are not used
    // don't know why ParentEntry needs to be null, but that's how it was in the ExportAllCode.csx script
    if (!code.Name.Content.Contains("ch1") && code.ParentEntry == null)
        allCode.Add(code);
}

SetProgressBar(null, "Code Entries", 0, allCode.Count);
StartProgressBarUpdater();
await SearchInCode(allCode);
await StopProgressBarUpdater();

foreach (string textCode in langJP)
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

File.WriteAllText(Path.Combine(langFolder, "lang_en.json"), jsonString);

var unused = new List<string>();

    foreach (string textCode in langJP)
{
    if (!langEN.ContainsKey(textCode))
    {
        unused.Add(textCode);
    }
}

// TO-DO: Make this path configurable?
    File.WriteAllLines(Path.Combine(langFolder, "unused.txt"), unused);

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
/// Task wrapper for <c>SearchInCode</c>
/// </summary>
/// <param name="allCode"></param>
/// <returns></returns>
async Task SearchInCode (List<UndertaleCode> allCode)
{
    await Task.Run(() => Parallel.ForEach(allCode, SearchInCode));
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
    var pattern = @$"(?<=^\([^""]*"")(\\""|[^""])*(?="")";
    return Regex.Match(functionCall, pattern).Value;
}

/// <summary>
/// Search inside a code entry for all the text codes and their corresponding language strings
/// </summary>
/// <param name="code"></param>
void SearchInCode (UndertaleCode code)
{
    var codeContent = Decompiler.Decompile(code, DECOMPILE_CONTEXT.Value);
    var codeLines = codeContent.Split("\n").ToArray();

    // keeping track of the possible so that we will know when we can skip a line for certain
    var possibleCodeCount = codeLines.Select(line => GetPossibleTextCodes(line)).ToArray();

    for (int i = 0; i < codeLines.Length; i++)
    {
        var line = codeLines[i];
        foreach (string textCode in langJP)
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
                                // the legendary one pipis textbox that has a pagebreak (bug)
                                if (textCode == "obj_pipis_enemy_slash_Step_0_gml_97_0")
                                    langString = langString.Replace("\f", "");
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