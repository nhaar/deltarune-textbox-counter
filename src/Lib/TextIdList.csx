using System.Linq;

/// <summary>
/// Create a text id list and add comments to all of them
/// </summary>
/// <param name="path"></param>
/// <param name="lines"></param>
/// <param name="langs"></param>
void WriteIdListWithComments (string path, List<string> lines, params Dictionary<string, string>[] langs)
{
    File.WriteAllLines(path, lines.Select(l => AddCommentToId(l, langs)).ToList());
}

/// <summary>
/// Add a comment to a text id
/// </summary>
/// <param name="textId"></param>
/// <param name="langs"></param>
/// <returns></returns>
string AddCommentToId (string textId, params Dictionary<string, string>[] langs)
{
    foreach (Dictionary<string, string> lang in langs)
    {
        if (lang.ContainsKey(textId))
        {
            return textId + " //" + lang[textId].Replace("\n", "\\n");
        }
    }
    return textId;
}

/// <summary>
/// Get a list of text ids from a text id file
/// </summary>
/// <param name="filePath"></param>
/// <returns></returns>
List<string> GetTextIdList (string filePath)
{
    var lines = File.ReadAllLines(filePath).ToList();
    var commentPattern = new Regex(@"(?<=^.*?)//.*$", RegexOptions.Multiline);
    lines = lines.Select(line => commentPattern.Replace(line, "")).ToList();
    lines = lines.Select(lines => lines.Trim()).ToList();
    lines.RemoveAll(line => line == "");
    return lines;
}