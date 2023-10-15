using System.Linq;

HashSet<string>[] GetJsonExclusive(params Dictionary<string, string>[] jsons)
{   
    List<HashSet<string>> sets = new();
    foreach (Dictionary<string, string> json in jsons)
    {
        sets.Add(new HashSet<string>());
    }

    for (int i = 0; i < jsons.Length; i++)
    {
        var json = jsons[i];

        foreach (string key in json.Keys)
        {
            
            var exclusive = true;
            for (int j = 0; j < jsons.Length; j++)
            {
                if (i != j)
                {
                    var otherJson = jsons[j];
                    if (otherJson.ContainsKey(key))
                    {
                        exclusive = false;
                        break;
                    }
                }
            }
            if (exclusive)
            {
                sets[i].Add(key);
            }
        }
    }

    return sets.ToArray();;
}