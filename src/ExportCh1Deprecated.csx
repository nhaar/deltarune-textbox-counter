using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;

EnsureDataLoaded();

ThreadLocal<GlobalDecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<GlobalDecompileContext>(() => new GlobalDecompileContext(Data, false));

var langFolder = Path.Combine(Path.GetDirectoryName(FilePath), "lang");

Dictionary<string, string> GetLang(string langName)
{
    ReadOnlySpan<byte> fileBytes = File.ReadAllBytes(Path.Combine(langFolder, $"lang_{langName}.json"));
    var reader = new Utf8JsonReader(fileBytes);
    Dictionary<string, string> lang = new();

    var lastProperty = "";
    while (reader.Read())
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.PropertyName:
                lastProperty = reader.GetString();
                break;
            case JsonTokenType.String:
                lang[lastProperty] = reader.GetString();
                break;
        }
    }
    return lang;
}

var langJP = GetLang("ja_ch1");
var langEN = GetLang("en_ch1");

var foundJP = new ConcurrentBag<string>();
var foundEN = new ConcurrentBag<string>();

List<UndertaleCode> ch1Code = Data.Code.Where(code => code.Name.Content.Contains("ch1") && code.ParentEntry == null).ToList();

SetProgressBar(null, "Extracting Text", 0, ch1Code.Count);
StartProgressBarUpdater();
await CheckCode();
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

void CheckCode (UndertaleCode code)
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

async Task CheckCode ()
{
    await Parallel.ForEachAsync(ch1Code, async (code, cancellationToken) => CheckCode(code));
}