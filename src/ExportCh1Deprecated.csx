#load "DecompileContext.csx"
#load "DeltarunePaths.csx"
#load "GetLang.csx"

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;

async Task ExportCh1Deprecated ()
{
    var langJP = GetLang("ja_ch1");
    var langEN = GetLang("en_ch1");

    var foundJP = new ConcurrentBag<string>();
    var foundEN = new ConcurrentBag<string>();

    List<UndertaleCode> ch1Code = Data.Code.Where(code => code.Name.Content.Contains("ch1") && code.ParentEntry == null).ToList();

    SetProgressBar(null, "Extracting Text", 0, ch1Code.Count);
    StartProgressBarUpdater();
    await Parallel.ForEachAsync(ch1Code, async (code, cancellationToken) => CheckCode(code, langJP, langEN, foundJP, foundEN));
    await StopProgressBarUpdater();

    var deprecated = new HashSet<string>();

    foreach (string textId in langJP.Keys)
    {
        if (!foundJP.Contains(textId))
        {
            var text = "";
            try
            {
                text = langEN[textId];
            }
            catch (System.Exception)
            {
                text = langJP[textId];
            }
            deprecated.Add(textId + " //" + text);
        }
    }

    foreach (string textId in langEN.Keys)
    {
        if (!foundEN.Contains(textId))
            deprecated.Add(textId + " //" + langEN[textId]);
    }

    File.WriteAllLines(Path.Combine(langFolder, "deprecated_ch1.txt"), deprecated);
}


void CheckCode (UndertaleCode code, Dictionary<string, string> langJP, Dictionary<string, string> langEN, ConcurrentBag<string> foundJP, ConcurrentBag<string> foundEN)
{
    var decompiled = Decompiler.Decompile(code, DECOMPILE_CONTEXT.Value);
    foreach (string textId in langJP.Keys)
    {
        if (decompiled.Contains($"\"{textId}\""))
            foundJP.Add(textId);
    }
    foreach (string textId in langEN.Keys)
    {
        if (decompiled.Contains($"\"{textId}\""))
            foundEN.Add(textId);
    }
    IncrementProgressParallel();
}