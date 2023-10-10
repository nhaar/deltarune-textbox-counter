using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;

// TO-DO: Ensure DELTARUNE DEMO 1&2 is loaded as well
EnsureDataLoaded();

// needed for decompiling the code entries
ThreadLocal<GlobalDecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<GlobalDecompileContext>(() => new GlobalDecompileContext(Data, false));

/// <summary>
/// Load the JSON for the Japanese language file as a dictionary
/// </summary>
/// <returns></returns>
// for some reason, the read only spans and utf8jsonreader only will work inside a function and not in the global scope
Dictionary<string, string> GetLangJP ()
{
    byte[] fileBytes = File.ReadAllBytes(FilePath + "/../lang/lang_ja.json");
    var reader = new Utf8JsonReader(new ReadOnlySpan<byte>(fileBytes));
    var langJP = new Dictionary<string, string>();

    // save last one because of the forward reading
    var lastProperty = "";
    while (reader.Read())
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.PropertyName:
            {
                lastProperty = reader.GetString();
                break;
            }
            case JsonTokenType.String:
            {
                langJP[lastProperty] = reader.GetString();
                break;
            }
        }
    }

    return langJP;
}

var langJP = GetLangJP();

var langEN = new ConcurrentDictionary<string, string>();

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

string jsonString = JsonSerializer.Serialize(langEN, new JsonSerializerOptions
{
    WriteIndented = true
});

// TO-DO: Make this path configurable?
// TO-DO: Unescape unicode
File.WriteAllText(FilePath + "/../lang/lang_en.json", jsonString);

var unused = new List<string>();

foreach (string textCode in langJP.Keys)
{
    if (!langEN.ContainsKey(textCode))
    {
        unused.Add(textCode);
    }
}

// TO-DO: Make this path configurable?
// TO-DO: "unused" might be a bad name for this file, since doesn't contain all unused strings, just a few
File.WriteAllLines(FilePath + "/../lang/unused.txt", unused);

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
/// <param name="functionName"></param>
/// <param name="functionCall"></param>
/// <returns></returns>
/// <remarks>
/// This function is used to get the language string from a function call, which is always
/// the first string argument of the function
/// </remarks>
string GetFirstArgument (string functionName, string functionCall)
{
    var pattern = @$"(?<={functionName}\([^""]*"").*?[^\\](?="")";
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
                var langString = "";
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
                    Match match = Regex.Match(line, @$"(?<=[^\w]|){functions[j]}\(.*?""{textCode}""\)");
                    if (match.Success)
                    {
                        langString = GetFirstArgument(functions[j], match.Value);
                        break;
                    }
                }

                // if the language string is empty, it means that we couldn't find the function call
                // so the it was not an actual text code, but a variable or something else
                if (langString != "")
                {
                    possibleCodeCount[i]--;
                    langEN[textCode] = langString;
                }
            }
        }
    }

    IncrementProgressParallel();
}