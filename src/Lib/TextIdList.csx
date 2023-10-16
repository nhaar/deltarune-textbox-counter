using System.Linq;

void WriteIdListWithComments (string path, List<string> lines, params Dictionary<string, string>[] langs)
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

List<string> GetTextIdList (string filePath)
{
    var lines = File.ReadAllLines(filePath).ToList();
    var commentPattern = new Regex(@"(?<=^.*?)//.*$", RegexOptions.Multiline);
    lines = lines.Select(line => commentPattern.Replace(line, "")).ToList();
    lines = lines.Select(lines => lines.Trim()).ToList();
    lines.RemoveAll(line => line == "");
    return lines;
}