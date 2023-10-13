using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;

EnsureDataLoaded();

ThreadLocal<GlobalDecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<GlobalDecompileContext>(() => new GlobalDecompileContext(Data, false));

var langFolder = Path.Combine(Path.GetDirectoryName(FilePath), "lang");

List<string> GetLang(string langName)
{
    ReadOnlySpan<byte> fileBytes = File.ReadAllBytes(Path.Combine(langFolder, $"lang_{langName}.json"));
    var reader = new Utf8JsonReader(fileBytes);
    List<string> lang = new();

    while (reader.Read())
    {
        // only need the keys
        if (reader.TokenType == JsonTokenType.PropertyName)
        {
            lang.Add(reader.GetString());
        }
    }
    return lang;
}

var langJP = GetLang("ja_ch1");
var langEN = GetLang("en_ch1");

Console.WriteLine(langJP.Count);

var foundJP = new ConcurrentBag<string>();
var foundEN = new ConcurrentBag<string>();

List<UndertaleCode> ch1Code = Data.Code.Where(code => code.Name.Content.Contains("ch1") && code.ParentEntry == null).ToList();

SetProgressBar(null, "Extracting Text", 0, ch1Code.Count);
StartProgressBarUpdater();
await CheckCode();
await StopProgressBarUpdater();

var deprecated = new HashSet<string>();

foreach (string textId in langJP)
{
    if (!foundJP.Contains(textId))
        deprecated.Add(textId);
}

foreach (string textId in langEN)
{
    if (!foundEN.Contains(textId))
        deprecated.Add(textId);
}

var union = langJP.Concat(langEN).ToList();
foreach (string textId in union)
{
    if (!langJP.Contains(textId) || !langEN.Contains(textId))
        deprecated.Add(textId)
}

File.WriteAllLines(Path.Combine(langFolder, "deprecated_ch1.txt"), deprecated);

void CheckCode (UndertaleCode code)
{
    var decompiled = Decompiler.Decompile(code, DECOMPILE_CONTEXT.Value);
    foreach (string textId in langJP)
    {
        if (decompiled.Contains($"\"{textId}\""))
            foundJP.Add(textId);
    }
    foreach (string textId in langEN)
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