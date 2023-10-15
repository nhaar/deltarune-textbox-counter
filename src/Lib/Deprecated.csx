#load "DecompileContext.csx"
#load "GetJson.csx"

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;

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

void CheckCode (UndertaleCode code, DeprecatedSearchObject[] targetFiles)
{
    var decompiled = Decompiler.Decompile(code, DECOMPILE_CONTEXT.Value);
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