#load "..\Lib\Deprecated.csx"
#load "..\Lib\AddCommentToId.csx"
#load "LangFile.csx"
#load "DeltaruneConstants.csx"
#load "DeltarunePaths.csx"

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;

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