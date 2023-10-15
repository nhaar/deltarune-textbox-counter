#load "..\Lib\Deprecated.csx"
#load "..\Lib\AddCommentToId.csx"
#load "LangFile.csx"
#load "DeltarunePaths.csx"
#load "GetLang.csx"

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;

async Task ExportDeltaruneDeprecated ()
{
    List<UndertaleCode> ch1Code = Data.Code.Where(code => code.Name.Content.Contains("ch1") && code.ParentEntry == null).ToList();
    var langEN = GetDeltaruneLangFile(Chapter.Chapter1, Lang.EN);
    var langJP = GetDeltaruneLangFile(Chapter.Chapter1, Lang.JP);
    var deprecated = await GetDeprecated(ch1Code.ToArray(), langEN, langJP);

    File.WriteAllLines
    (
        Path.Combine(langFolder, "deprecated_ch1.txt"),
        deprecated.Select(textId => AddCommentToId(textId, langEN, langJP))
    );
}