using System.Linq;

List<string> GetFileTextList (string filePath)
{
    var lines = File.ReadAllLines(filePath).ToList();
    var commentPattern = new Regex(@"(?<=^.*?)//.*$", RegexOptions.Multiline);
    lines = lines.Select(line => commentPattern.Replace(line, "")).ToList();
    lines = lines.Select(lines => lines.Trim()).ToList();
    lines.RemoveAll(line => line == "");
    return lines;
}