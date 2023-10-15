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