using System.Linq;

void WriteWithComments (string path, List<string> lines, params Dictionary<string, string>[] langs)
{
    File.WriteAllLines(path, lines.Select(l => AddCommentToId(l, langs)).ToList());
}

string AddCommentToId (string textId, params Dictionary<string, string>[] langs)
{
    foreach (Dictionary<string, string> lang in langs)
    {
        if (lang.ContainsKey(textId))
        {
            return textId + " //" + lang[textId];
        }
    }
    return textId;
}