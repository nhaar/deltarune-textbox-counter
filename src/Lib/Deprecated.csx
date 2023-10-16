#load "DecompileContext.csx"
#load "JsonUtils.csx"

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;

/// <summary>
/// Get all deprecated text ids
/// </summary>
/// <param name="codeList"></param>
/// <param name="langs"></param>
/// <returns></returns>
async Task<HashSet<string>> GetDeprecated (UndertaleCode[] codeList, params Dictionary<string, string>[] langs)
{
    var langFiles = langs.Select(l => new DeprecatedSearchObject(l)).ToArray();
    SetProgressBar(null, "Extracting Deprecated Text", 0, codeList.Length);
    StartProgressBarUpdater();
    while (true)
    {
        try
        {
            await Parallel.ForEachAsync(codeList, async (code, cancellationToken) => CheckCode(code, langFiles));
            break;       
        }
        catch (System.Exception)
        {
        }
    }
    await StopProgressBarUpdater();
    var deprecated = new HashSet<string>();

    foreach (DeprecatedSearchObject langFile in langFiles)
    {
        foreach (string textId in langFile.Lang.Keys)
        {
            if (!langFile.Found.Contains(textId))
                deprecated.Add(textId);
        }
    }
    return deprecated;
}

/// <summary>
/// Holds both original and found text ids
/// </summary>
class DeprecatedSearchObject
{
    public Dictionary<string, string> Lang;

    public ConcurrentBag<string> Found;

    public DeprecatedSearchObject (Dictionary<string, string> lang)
    {
        Lang = lang;
        Found = new ConcurrentBag<string>();
    }
}

/// <summary>
/// Check if a code contains a text id from the target files
/// </summary>
/// <param name="code"></param>
/// <param name="targetFiles"></param>
void CheckCode (UndertaleCode code, DeprecatedSearchObject[] targetFiles)
{
    var decompiled = Decompile(code);
    foreach (DeprecatedSearchObject targetFile in targetFiles)
    {
        foreach (string textId in targetFile.Lang.Keys)
        {
            if (decompiled.Contains($"\"{textId}\""))
                targetFile.Found.Add(textId);
        }
    }
    IncrementProgressParallel();
}