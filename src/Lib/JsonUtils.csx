using System.Text.Json;
using System.Linq;
using System.Text.Encodings.Web;

void ExportJson (Dictionary<string, string> json, string path)
{
    string jsonString = JsonSerializer.Serialize(json, new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });

    File.WriteAllText(path, jsonString);
}

/// <summary>
/// Load the JSON for a file as a dictionary
/// </summary>
/// <returns></returns>
// for some reason, the read only spans and utf8jsonreader only will work inside a function and not in the global scope
Dictionary<string, string> GetJsonAsDict (string filePath)
{
    ReadOnlySpan<byte> fileBytes = File.ReadAllBytes(filePath);
    var reader = new Utf8JsonReader(fileBytes);
    Dictionary<string, string> json = new();

    var lastProperty = "";
    while (reader.Read())
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.PropertyName:
                lastProperty = reader.GetString();
                break;
            case JsonTokenType.String:
                json[lastProperty] = reader.GetString();
                break;
        }
    }
    return json;
}

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